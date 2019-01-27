using UnityEngine;
using System.Collections;

public class AudioHandler : PersistentSceneSingleton<AudioHandler>
{

    AudioSource[] mSound; // = new AudioSource[20];
    AudioSource[] mMusicPlayers;

    public const int SOUND_SEAGULL = 0;

    public const int MUSIC_OCEAN = 0;

    public AudioClip[] mSoundClips;
    public AudioClip[] mMusicClips;

    public float[] mSoundVolumes;

    public override void Awake()
    {

        base.Awake();

        mSound = new AudioSource[mSoundClips.Length];

        for (int i = 0; i < mSound.Length; i++)
        {
            mSound[i] = gameObject.AddComponent<AudioSource>();
            mSound[i].clip = mSoundClips[i];
        }

        mMusicPlayers = new AudioSource[mMusicClips.Length];

        for (int i = 0; i < mMusicPlayers.Length; i++)
        {
            mMusicPlayers[i] = gameObject.AddComponent<AudioSource>();
            mMusicPlayers[i].clip = mMusicClips[i];
            mMusicPlayers[i].volume = 0;
            mMusicPlayers[i].loop = true;
        }

        SetVolume();
    }

    public bool mStartImmediately;

    public void SetSong(string songPath)
    {
        mMusicClips[0] = (AudioClip)Resources.Load("Tracks/" + songPath);
        mMusicPlayers = new AudioSource[mMusicClips.Length];

        for (int i = 0; i < mMusicPlayers.Length; i++)
        {
            mMusicPlayers[i] = gameObject.AddComponent<AudioSource>();
            mMusicPlayers[i].clip = mMusicClips[i];
            mMusicPlayers[i].volume = 0;
            mMusicPlayers[i].loop = true;
        }

        SetVolume();
    }

    // Use this for initialization
    void Start()
    {
        //:NOTE: there are a limited number of sound audio sources (15)
        //GameStateManager.mInstance.AddTransition(GameStateManager.GameTransition.OPEN_BOOSTERSHOP, Transition_PlayBoosterShopMusic);
        //GameStateManager.mInstance.AddTransition(GameStateManager.GameTransition.OPEN_LEVELSUMMARY, Transition_LevelSummary);
        //GameStateManager.mInstance.AddTransition(GameStateManager.GameTransition.RESTART_LEVEL, Transition_RestartLevel);
        //GameStateManager.mInstance.AddTransition(GameStateManager.GameTransition.LOAD_MAINMENU, Transition_PlayMenuMusic);
        //GameStateManager.mInstance.AddTransition(GameStateManager.GameTransition.PLAY_GAME, Transition_PlayGameplayMusic);
        if (mStartImmediately)
            PlayMusic(AudioHandler.MUSIC_OCEAN);
    }

    void Transition_PlayBoosterShopMusic()
    {
        //if (Constants.mBattleName.Equals("EnchantedForest") && Constants.mCurrentLevelIndex == 1) return;

        //if (Constants.mBattleName.Equals("HomeMeadows"))
        //    Transition_PlayGameplayMusic();
        //else
        //    PlayMusic(MUSIC_MENU);

        //Debug.LogWarning("BOOSTERMUSIC");
    }

    void Transition_PlayMenuMusic()
    {
        // PlayMusic(MUSIC_MENU);
    }

    public void Transition_LevelSummary()
    {
        StartCoroutine(FadeOut());
    }

    //void Transition_RestartLevel()
    //{
    //    if(Constants.mBattleName.Equals("HomeMeadows"))

    //}

    IEnumerator FadeOut()
    {
        bool ready = false;

        while (!ready)
        {
            for (int i = 0; i < mMusicPlayers.Length; i++)
            {
                mMusicPlayers[i].volume -= 0.30f * Time.deltaTime;

                if (mMusicPlayers[i].isPlaying && mMusicPlayers[i].volume <= 0f)
                    ready = true;
            }

            yield return null;
        }

        for (int i = 0; i < mMusicPlayers.Length; i++)
            mMusicPlayers[i].Stop();
    }

    public void Transition_PlayGameplayMusic()
    {
        //switch (Constants.mBattleName)
        //{
        //    case "HomeMeadows":
        //        PlayMusic(MUSIC_GAMEPLAY);
        //        break;
        //    case "EnchantedForest":
        //        PlayMusic(MUSIC_ENCHANTED);
        //        break;
        //    case "GhostMountain":
        //        PlayMusic(MUSIC_GHOST);
        //        break;
        //    case "DeadPlains":
        //        PlayMusic(MUSIC_DEADPLAINS);
        //        break;
        //    default:
        //        PlayMusic(MUSIC_GAMEPLAY);
        //        break;
        //}
    }

    // :TODO: this is convenient, but a bit stupid, as the more music tracks we have, the longer of strucure we need
    // Change this so that the music in and out is selected in the function call parameters already 
    public void PlayMusic(int index)
    {
        bool playing = false;

        for (int i = 0; i < mMusicPlayers.Length; i++)
        {
            if (mMusicPlayers[i].isPlaying && index != i)
            {
                StartCoroutine(FadeIn(index, i));
                playing = true;
                break;
            }
        }

        //Start the music if no music playing
        if (!playing && !mMusicPlayers[index].isPlaying)
        {
            StartCoroutine(FadeIn(index, -1));
        }
    }

    public double mFadeDelay = 2.0;

    IEnumerator FadeIn(int music_in, int music_out = -1)
    {
        AudioSource mMusicPlayerIn;
        AudioSource mMusicPlayerOut;

        double time_prev = Time.time;
        double time_delta = 0.0;

        if (music_out == -1)
            music_out = music_in;

        float tempMusicVol = PlayerPrefs.GetFloat("musicVolume", 1f);

        mMusicPlayerIn = mMusicPlayers[music_in];
        mMusicPlayerOut = mMusicPlayers[music_out];

        for (double d = 0f; d < mFadeDelay; d += time_delta)
        {
            time_delta = Time.time - time_prev;
            time_prev = Time.time;

            if (d < mFadeDelay / 2)
            {
                if (!mMusicPlayerOut.isPlaying) d = mFadeDelay / 2f;
                mMusicPlayerOut.volume = tempMusicVol * (float)((mFadeDelay / 2.0 - d) / (mFadeDelay / 2.0));
            }
            else
            {
                if (!mMusicPlayerIn.isPlaying)
                {
                    if (mMusicPlayerOut.isPlaying) mMusicPlayerOut.Stop();
                    mMusicPlayerIn.Play();
                    mMusicPlayerIn.volume = 0f;
                }
                mMusicPlayerIn.volume = tempMusicVol * (float)((d - mFadeDelay / 2.0) / (mFadeDelay / 2.0));
            }

            yield return null;
        }
    }

    public void SetInitialVolumes()
    {
        PlayerPrefs.SetFloat("musicVolume", 0.5f);
        PlayerPrefs.SetFloat("soundVolume", 0.5f);
    }

    public void SetVolume()
    {
        float tempMusicVol = PlayerPrefs.GetFloat("musicVolume", 0.5f);
        float tempSoundVol = PlayerPrefs.GetFloat("soundVolume", 0.5f);

        foreach (AudioSource music in mMusicPlayers)
            music.volume = tempMusicVol;

        foreach (AudioSource sound in mSound)
            sound.volume = tempSoundVol;
    }

    public void PlaySound(int index)
    {
        float tempSoundVol = PlayerPrefs.GetFloat("soundVolume", 0.5f);
        for (int i = 0; i < mSound.Length; i++)
        {
            if (mSound[i].isPlaying) continue;

            mSound[i].volume = tempSoundVol * mSoundVolumes[index];
            mSound[i].clip = mSoundClips[index];
            mSound[i].Play();
            break;
        }
    }
}