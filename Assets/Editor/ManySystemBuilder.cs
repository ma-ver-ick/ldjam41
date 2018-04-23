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

                if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64) {
                    locationPathName += ".exe";
                }

                var options = new BuildPlayerOptions {
                    locationPathName = locationPathName,
                    scenes = new[] {
                        "Assets/Scenes/Menu.unity",
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