using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Builder {
    public class ManySystemBuilder : MonoBehaviour {
        public static readonly BuildTarget[] TARGETS = {
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.StandaloneLinuxUniversal,
            BuildTarget.WebGL
        };

        public static void build() {
            foreach (var target in TARGETS) {
                var locationPathName = "builds/" + ("" + target).ToLower().Replace("standalone", "") + "/residentracing";

                var options = new BuildPlayerOptions {
                    locationPathName = locationPathName,
                    scenes = new[] {
                        "Assets/Scenes/RaceTrack01.unity"
                    },
                    options = BuildOptions.None,
                    target = target
                };

                BuildPipeline.BuildPlayer(options);
            }
        }
    }
}