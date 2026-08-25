using NUnit.Framework;
using System.Collections.Generic;
using Unity.Services.Multiplayer.Components;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum SE
    {
        MoveScene = 0,
        Main_text_chenge,
        Main_text_voice,
        Main_crown_add,
        Main_miniGame_select,
        Main_miniGeme_decision,
        MiniGame_CountDown,
        MiniGame_Start,
        MiniGame_Finish,
        //Bank
        Dial_Open,
        Bank_Open,
        //Click
        Button_Click,
        Bomb_Catch,
        Garage_Open,
        Spatto_cut,
    }

    public enum BGM
    {
        Title = 0,
        Main_Normal,
        Main_End,
        Nit_Nit,
        Garage,
        Bom_doge,
        Arcana,
        Bank,
        Bowling,
        Spatto,
    }

    [SerializeField] public List<AudioClip> seClips = new List<AudioClip>();
    static List<AudioClip> SEs = new List<AudioClip>();

    [SerializeField] public List<AudioClip> bgmClips = new List<AudioClip>();
    static List<AudioClip> BGMs = new List<AudioClip>();


    static AudioSource audioSource;

    static public bool isStartBgm; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        SEs = seClips;
        BGMs = bgmClips;
        isStartBgm = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static public void PlaySE(SE type)
    {
        audioSource.PlayOneShot(SEs[(int)type]);
    }

    static public void ChangeBGM(BGM type)
    {
        audioSource.clip = BGMs[(int)type];
        audioSource.Play();
        audioSource.loop = true;
        isStartBgm = true;
    }

    static public void StopBgm()
    {
        audioSource.Stop();
        isStartBgm = false;
    }

}
