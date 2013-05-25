// Copyright (c) 2012 Eric Salczynski and Ramón Rocha
// This program (TextGen) is released under the MIT License.
// http://opensource.org/licenses/MIT

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TextGen : EditorWindow {
    public Object file = null;
    public bool deleteAfterImport = false;

    [MenuItem("GameObject/Import From TextGen File...", false, 9999)]
    public static void ImportFromTextGenFile() {
        EditorWindow.GetWindow(typeof(TextGen));
    }

    public void OnGUI() {
        GUILayout.Label("Import a Text Gen File", EditorStyles.boldLabel);
        file = EditorGUILayout.ObjectField("Text Gen File", file, typeof(Object), false);
        EditorGUIUtility.LookLikeControls(283.0f);
        deleteAfterImport = EditorGUILayout.Toggle("Delete template instances after import?", deleteAfterImport);

        GUILayout.BeginArea(new Rect(240, 65, 100, 100));
        if (GUILayout.Button("Import!", GUILayout.Width(60))) {
            DoImport();
            this.Close();
        }
        GUILayout.EndArea();
    }

    public void DoImport() {
        if (file != null) {
            // Let's find our template objects in the hierarchy
            Object[] allSprites = FindObjectsOfType(typeof(Sprite));
            Hashtable templateSprites = new Hashtable();    // Hashtable of our found Sprite GameObject templates
            Hashtable templateParents = new Hashtable();    // Empty GameObjects to act as parents for new instances
            foreach (Object sprite in allSprites) {
                if (((Sprite)sprite).gameObject.tag == "TextGenTemplate") {
                    string spriteName = ((Sprite)sprite).name;
                    templateSprites.Add(spriteName, sprite);
                }
            }
            
            if (templateSprites.Count == 0) {
                throw new System.Exception("No Sprite templates found. Be sure to tag templates with 'TextGenTemplate'.");
            }
            
            TextAsset textGenFile = (TextAsset)file;    // Our file is actually a text file
            bool recording = false;                     // We've hit the map start config option
            GameObject parentContainer = null;          // If specified, add all instances to this parent
            int scale = 1;                              // Grid size
            int globalDepth = 0;                        // Depth at which to add the parent (or each individual instance)
            List<string> map = new List<string>();      // Array of lines representing our map
            Hashtable keyReference = new Hashtable();   // Hash of key references ex: {"@" => "MarioSprite", "T" => "Ground", ... }

            string[] result = Regex.Split(textGenFile.text, "\r?\n");
            foreach (string line in result) {
                if (line.StartsWith(";=")) {    // Key reference line
                    string[] keyPair = line.Replace(";=", "").Split(':');
                    string[] values = Regex.Split(keyPair[1], @",\s*");
                    string spriteName = values[0];
                    string containerName = spriteName + "Container";
                    Vector3 containerTransform = Vector3.zero;

                    // Get all the settings for this instance's parent container
                    keyReference.Add(keyPair[0], spriteName);
                    foreach (string attributeValue in values) {
                        if (attributeValue.StartsWith("z=")) {    // instance container z-depth
                            int depth = int.Parse(attributeValue.Replace("z=", ""));
                            containerTransform = new Vector3(0, 0, depth);
                        }
                        if (attributeValue.StartsWith("n=")) {    // instance container name
                            containerName = attributeValue.Replace("n=", "");
                        }
                    }

                    // Try to find the parent in the hierarchy. If it doesn't exist yet,
                    // create it, and apply the settings we found from above.
                    GameObject templateParent = GameObject.Find(containerName);
                    if (templateParent == null) {
                        templateParent = new GameObject(containerName);
                        templateParent.transform.position = containerTransform;
                    }

                    // Add our container to the hashtable for lookup later.
                    templateParents.Add(spriteName, templateParent);
                } else if (line.StartsWith(";s=")) {    // Scale
                    scale = int.Parse(line.Replace(";s=", ""));
                } else if (line.StartsWith(";n=")) {    // parent container
                    string parentContainerName = line.Replace(";n=", "");
                    parentContainer = GameObject.Find(parentContainerName);
                    if (parentContainer == null) {
                        parentContainer = new GameObject(parentContainerName);
                    }
                } else if (line.StartsWith(";z=")) {   // global z-depth
                    globalDepth = int.Parse(line.Replace(";z=", ""));
                } else if (line.StartsWith(";+")) {    // Start of map definition
                    recording = true;
                } else if (recording && !line.StartsWith(";")) {    // Map definition lines
                    map.Add(line);
                }
            }
            
            map.Reverse();    // Start from the bottom and work our way up
            for (var y = 0; y < map.Count; y++) {
                string line = map[y];
                
                for (var x = 0; x < line.Length; x++) {
                    string key = line[x].ToString();
                    if (key != " " && key != "`") {  // ignore spaces and back ticks (`)
                        Sprite sprite = (Sprite)templateSprites[keyReference[key]];    // The template sprite we want to dupe

                        // We want to use InstantiatePrefab because we want to maintain the link to the
                        // prefab object when we add it to the hierarchy. Instantiate creates a clone of
                        // a GameObject and breaks the connection to the prefab.
                        Object prefabRoot = PrefabUtility.GetPrefabParent(sprite.gameObject);
                        Object dupe = PrefabUtility.InstantiatePrefab(prefabRoot);
                        
                        // Make our dupe look like our template
                        dupe.name = sprite.gameObject.name;
                        Sprite dupeSprite = ((GameObject)dupe).GetComponent<Sprite>();
                        dupeSprite.spriteContainer = sprite.spriteContainer;
                        dupeSprite.frameIndex = sprite.frameIndex;
                        
                        if (parentContainer != null) {
                            ((GameObject)dupe).transform.parent = parentContainer.transform;
                        }
                        
                        // Add our instance to its parent GameObject
                        ((GameObject)dupe).transform.parent = ((GameObject)templateParents[keyReference[key]]).transform;

                        // Set the new instance's position. The kindred-sprite manager has a center pivot
                        // point, but the text-gen assumes a lower-left offset, so we need to adjust.
                        float xOffset = sprite.renderer.bounds.extents.x;
                        float yOffset = sprite.renderer.bounds.extents.y;
                        ((GameObject)dupe).transform.localPosition = new Vector3(x * scale + xOffset, y * scale + yOffset, 0);
                    }
                }
            }
            
            
            // Make necessary adjustments to our instance containers
            foreach (DictionaryEntry pair in templateParents) {
                GameObject templateParent = (GameObject)pair.Value;
                if (parentContainer != null) {
                    // If the global parent exists, add our instance containers to it
                    templateParent.transform.parent = parentContainer.transform;
                }
                
                // Set the instance container depth to the globalDepth if the instance container's
                // depth wasn't specified.
                if (templateParent.transform.position.z == 0) {
                    templateParent.transform.localPosition = new Vector3(0, 0, globalDepth);
                }
            }

            if (deleteAfterImport) {
                // Destroy the template instances
                foreach (DictionaryEntry pair in templateSprites) {
                    DestroyImmediate(((Sprite)pair.Value).gameObject);
                }
            }
        }
    }
}

#endif