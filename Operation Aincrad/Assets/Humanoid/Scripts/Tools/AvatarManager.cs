using UnityEngine;

namespace Passer {

    [HelpURLAttribute("https://passervr.com/documentation/humanoid-control/avatar-manager/")]
    public class AvatarManager : MonoBehaviour {

        public int currentAvatarIndex = 0;
        public Animator[] fpAvatars = new Animator[0];
        public Animator[] tpAvatars = new Animator[0];

        private HumanoidControl humanoid;

        void Start() {
            humanoid = GetComponent<HumanoidControl>();
            SetAvatar(currentAvatarIndex);
        }

        public void NextAvatar() {
            currentAvatarIndex = mod(currentAvatarIndex + 1, fpAvatars.Length);
            SetAvatar(currentAvatarIndex);
        }

        public void PreviousAvatar() {
            currentAvatarIndex = mod(currentAvatarIndex - 1, fpAvatars.Length);
            SetAvatar(currentAvatarIndex);
        }

        public void SetAvatar(int avatarIndex) {
            if (fpAvatars[avatarIndex] != null) {
#if hNW_UNET || hNW_PHOTON
            if (avatarIndex < tpAvatars.Length && tpAvatars[avatarIndex] != null)
                humanoid.ChangeAvatar(fpAvatars[avatarIndex].gameObject, tpAvatars[avatarIndex].gameObject);
            else
#endif
                humanoid.ChangeAvatar(fpAvatars[avatarIndex].gameObject);
            }
        }

        private int mod(int k, int n) {
            k %= n;
            return (k < 0) ? k + n : k;
        }
    }
}