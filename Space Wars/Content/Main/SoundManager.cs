using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using System.Diagnostics;
using Microsoft.Xna.Framework.Media;

namespace Space_Wars.Content.Main;
public static class SoundManager
{
    private static List<SoundEffectInstance> sounds = [];
    private static List<SoundEffectInstance> loopedSounds = [];
    private static Random random = new();
    private static Player player => Engine.SaveGame.Player;
    private static SoundEffectInstance currentTrack;
    private static SoundEffectInstance prevTrack;
    private static float soundTimer = -1;
    private const float swapTime = 3;
    private static float musicVolume = 0;
    public static float MusicVolume { get { return musicVolume; } set { musicVolume = value; UpdateVolume(); } }
    private static float sfxVolume = 1;
    public static float SFXVolume { get { return sfxVolume; } set { sfxVolume = value; UpdateVolume(); } }
    public static void Initialize()
    {
        SetAllSounds(false);
        sounds.Clear();
        loopedSounds.Clear();
    }
    public static void PlayLoopedSound(SoundEffectInstance _sound)
    {
        if (!loopedSounds.Contains(_sound))
        {
            _sound.Volume = sfxVolume;
            loopedSounds.Add(_sound);
            _sound.Play();
        }
        else
        {
            _sound.Resume();
        }
    }
    private static void UpdateVolume()
    {
        foreach (var sfx in sounds)
        {
            sfx.Volume = sfxVolume;
        }
        if (currentTrack != null)
        {
            currentTrack.Volume = musicVolume;
        }
    }
    public static void SetSoundVolume(SoundEffectInstance _sound, float _volume)
    {
        _sound.Volume = _volume * sfxVolume;
    }
    public static void PlaySound(SoundEffect _sound, Vector2 _playLocation)
    {
        Vector2 listenerLocation = player.position;
        float distance = MathF.Sqrt(MathF.Pow(_playLocation.X - listenerLocation.X, 2) + MathF.Pow(_playLocation.Y - listenerLocation.Y, 2));
        float volume = MathF.Max(0, -(distance / 1000) + 1) * SFXVolume;
        //float pan = -(listenerLocation.X - playLocation.X) / (screenSize.X);
        //Monogame issue, audio cannot pan smoothly
        SoundEffectInstance soundInstance = _sound.CreateInstance();
        soundInstance.Volume = volume;
        soundInstance.Pitch = ((float)random.NextDouble() - 0.5f) / 8f;
        sounds.Add(soundInstance);
        soundInstance.Play();
    }
    public static void PlayGlobalSound(SoundEffect sound)
    {
        sound.Play(SFXVolume, 0, 0);
    }
    public static void SetAllSounds(bool _playingSounds)
    {
        if(_playingSounds)
        {
            foreach (var soundEffect in sounds)
            {
                soundEffect.Resume();
            }
            foreach (var soundEffect in loopedSounds)
            {
                soundEffect.Resume();
            }
        }
        else
        {
            foreach (var soundEffect in sounds)
            {
                soundEffect.Pause();
            }
            foreach (var soundEffect in loopedSounds)
            {
                soundEffect.Pause();
            }
        }
    }
    public static void Update()
    {
        //Clears out completed sounds
        sounds = sounds.Where(sound => !(sound.State == SoundState.Stopped)).ToList();
        loopedSounds = loopedSounds.Where(sound => !(sound.State == SoundState.Stopped)).ToList();
        foreach (var sound in loopedSounds)
        {
            sound.Pause();
        }

        if (soundTimer > 0)
        {
            currentTrack.Volume = (swapTime - soundTimer) * MusicVolume / swapTime;
            if (prevTrack != null)
            {
                prevTrack.Volume = soundTimer * MusicVolume / swapTime;
            }
            soundTimer -= Engine.DeltaSeconds;
        }
        else if (soundTimer != -1)
        {
            soundTimer = -1;
            if (prevTrack != null)
            {
                prevTrack.Stop();
                prevTrack = null;
            }
        }
    }
    public static void ChangeTrack(SoundEffect _track)
    {
        prevTrack?.Pause();
        SoundEffectInstance track = _track.CreateInstance();
        track.IsLooped = true;
        track.Volume = MusicVolume;
        prevTrack = currentTrack;
        currentTrack = track;
        currentTrack.Play();
        soundTimer = swapTime;
    }
}
