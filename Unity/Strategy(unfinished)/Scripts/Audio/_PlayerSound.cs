using UnityEngine;
namespace sound.playerSound
{
    public class _PlayerSound : MonoBehaviour
    {
        public static _PlayerSound instance;

        [Header("#PlayerSFX")]
        public AudioClip[] plyerSfxClip;
        [Header("#PlayerBGM")]
        public AudioClip[] plyerBgmClip;
        public static float playerSfxVolume = 0.5f;
        public static float playerBgmVolume = 0.3f;
        public int channels;
        public int bgmChannels;
        AudioSource []playerSfxPlayers;
        AudioSource []playerBgmPlayer;
        int channelIndex;
        int bgmIndex;

        //이곳에 사용할 사운드를 지정
        public enum PlayerSfx {
            Hit,//0
            FootSand1,//1
            FootSand2,//2
            FootSand3,//3
            FootSand4,//4
            FootAspalt1,//5
            FootAspalt2,//6
            }
        public enum PlayerBgm {
            Bgm1,//0
            Bgm2,//1
            Bgm3//2
        }
        void Awake(){
            instance = this;
            Init();
        }

        public void Init()
        {
            //효과음 플레이어 초기화
            GameObject sfxObject = new GameObject("_PlayerSound");
            sfxObject.transform.parent = transform;
            playerSfxPlayers = new AudioSource[channels];
            //배경음 플레이어 초기화
            GameObject bgmObject = new GameObject("_PlayerBgm");
            bgmObject.transform.parent = transform;
            playerBgmPlayer = new AudioSource[bgmChannels];

            for (int i = 0; i < playerSfxPlayers.Length; i++)
            {
                playerSfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
                playerSfxPlayers[i].playOnAwake = false;
                playerSfxPlayers[i].volume = playerSfxVolume;
            }

            for (int i = 0; i < playerBgmPlayer.Length; i++)
            {
                playerBgmPlayer[i] = bgmObject.AddComponent<AudioSource>();
                playerBgmPlayer[i].playOnAwake = false;
                playerBgmPlayer[i].loop = true;
                playerBgmPlayer[i].volume = playerBgmVolume;
            }
        }

        public void PlayPlayerSFX(PlayerSfx sfx){
            for(int i = 0; i < playerSfxPlayers.Length; i++){
                int loopIndex = (i + channelIndex)%playerSfxPlayers.Length;
                
                if(playerSfxPlayers[loopIndex].isPlaying)
                    continue;

                    channelIndex = loopIndex;
                    playerSfxPlayers[loopIndex].clip = plyerSfxClip[(int)sfx];
                    playerSfxPlayers[loopIndex].Play();
                    break;
            }        
        }
        public void StopPlayerSFX(PlayerSfx sfx){
            for(int i = 0; i < playerSfxPlayers.Length; i++){
                if(playerSfxPlayers[i].clip == plyerSfxClip[(int)sfx] && playerSfxPlayers[i].isPlaying){
                    playerSfxPlayers[i].Stop();
                    break;
                }
            }
        }
        public void PlayPlayerBGM(PlayerBgm bgm){
            for(int i = 0; i < playerBgmPlayer.Length; i++){
                int loopIndex = (i + bgmIndex)%playerBgmPlayer.Length;
                
                if(playerBgmPlayer[loopIndex].isPlaying)
                    continue;

                    bgmIndex = loopIndex;
                    playerBgmPlayer[loopIndex].clip = plyerBgmClip[(int)bgm];
                    playerBgmPlayer[loopIndex].Play();
                    break;
            }        
        }
        public void StopPlayerBGM(PlayerBgm bgm){
            for(int i = 0; i < playerBgmPlayer.Length; i++){
                if(playerBgmPlayer[i].clip == plyerBgmClip[(int)bgm] && playerBgmPlayer[i].isPlaying){
                    playerBgmPlayer[i].Stop();
                    break;
                }
            }
        }
    }
}