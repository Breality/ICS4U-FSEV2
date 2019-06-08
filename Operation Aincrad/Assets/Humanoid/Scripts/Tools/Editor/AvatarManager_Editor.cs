using UnityEngine;
using UnityEditor;

namespace Passer {
    [CustomEditor(typeof(AvatarManager))]
    public class AvatarManager_Editor : Editor {

        public void OnDisable() {
            AvatarManager avatarManager = (AvatarManager)target;
            Cleanup(avatarManager);
        }

        public override void OnInspectorGUI() {
            AvatarManager avatarManager = (AvatarManager)target;

            CurrentAvatarInspector(avatarManager);

            if (avatarManager.tpAvatars.Length < avatarManager.fpAvatars.Length)
                avatarManager.tpAvatars = Extend(avatarManager.tpAvatars, avatarManager.fpAvatars.Length);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("#", GUILayout.Width(20));
            EditorGUILayout.LabelField("First Person", GUILayout.MinWidth(100));
#if hNW_UNET || hNW_PHOTON
        EditorGUILayout.LabelField("Third Person", GUILayout.MinWidth(100));
#endif
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < avatarManager.fpAvatars.Length; i++)
                AvatarInspector(avatarManager, i);

            if (GUILayout.Button("Add Avatar"))
                AddAvatar(avatarManager);
        }

        private void CurrentAvatarInspector(AvatarManager avatarManager) {
            int lastAvatarIndex = avatarManager.currentAvatarIndex;
            avatarManager.currentAvatarIndex = EditorGUILayout.IntField("Current Avatar Index", avatarManager.currentAvatarIndex);
            if (Application.isPlaying && avatarManager.currentAvatarIndex != lastAvatarIndex)
                avatarManager.SetAvatar(avatarManager.currentAvatarIndex);
        }

        private void AvatarInspector(AvatarManager avatarManager, int i) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(20));
            avatarManager.fpAvatars[i] = (Animator)EditorGUILayout.ObjectField(avatarManager.fpAvatars[i], typeof(Animator), false);
#if hNW_UNET || hNW_PHOTON
        avatarManager.tpAvatars[i] = (Animator) EditorGUILayout.ObjectField(avatarManager.tpAvatars[i], typeof(Animator), false);
#endif
            EditorGUILayout.EndHorizontal();
        }

        private void AddAvatar(AvatarManager avatarManager) {
            avatarManager.fpAvatars = Extend(avatarManager.fpAvatars, avatarManager.fpAvatars.Length + 1);
        }

        private Animator[] Extend(Animator[] animators, int n) {
            Animator[] newAnimators = new Animator[n];

            for (int i = 0; i < animators.Length; i++)
                newAnimators[i] = animators[i];

            return newAnimators;
        }

        private void Cleanup(AvatarManager avatarManager) {
            int nNonNullEntries = 0;
            foreach (Animator animator in avatarManager.fpAvatars) {
                if (animator != null)
                    nNonNullEntries++;
            }

            if (nNonNullEntries == avatarManager.fpAvatars.Length)
                return;

            Animator[] newFpAvatars = new Animator[nNonNullEntries];
            int j = 0;
            for (int i = 0; i < avatarManager.fpAvatars.Length; i++) {
                if (avatarManager.fpAvatars[i] != null) {
                    newFpAvatars[j] = avatarManager.fpAvatars[i];
                    j++;
                }
            }
            avatarManager.fpAvatars = newFpAvatars;
        }
    }
}