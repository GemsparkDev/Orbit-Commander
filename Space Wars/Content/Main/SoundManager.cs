using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Space_Wars.Content.Main;
public static class SoundManager
{
    private static List<SoundEffectInstance> sounds = [];
    private static List<SoundEffectInstance> loopedSounds = [];
    private static List<SoundEffectInstance> loopsToStop = [];
    private static Random random = new();
    private static Player Player => Engine.SaveGame.Player;
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
            _sound.IsLooped = true;
            loopedSounds.Add(_sound);
        }
        if (_sound.State != SoundState.Playing)
        {
            _sound.Play();
        }
        loopsToStop.Remove(_sound);
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
        Vector2 listenerLocation = Player.Position;
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
    public static void PlayGlobalSound(SoundEffectInstance sound)
    {
        sound.Volume *= SFXVolume;
        sound.Play();
    }
    public static void SetAllSounds(bool _playingSounds)
    {
        if (_playingSounds)
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
        sounds = [.. sounds.Where(sound => !(sound.State == SoundState.Stopped))];
        loopedSounds = [.. loopedSounds.Where(sound => !(sound.State == SoundState.Stopped))];
        loopsToStop = [.. loopsToStop.Where(sound => !(sound.State == SoundState.Stopped))];
        foreach (var sound in loopsToStop)
        {
            sound.Pause();
        }
        foreach (var sound in loopedSounds)
        {
            if (!loopsToStop.Contains(sound))
            {
                loopsToStop.Add(sound);
            }
        }

        if (soundTimer > 0)
        {
            if (currentTrack != null)
            {
                currentTrack.Volume = (swapTime - soundTimer) * MusicVolume / swapTime;
            }
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
        prevTrack = currentTrack;
        if (_track != null)
        {
            SoundEffectInstance track = _track.CreateInstance();
            track.IsLooped = true;
            track.Volume = MusicVolume;
            currentTrack = track;
            currentTrack.Play();
        }
        else
        {
            currentTrack = null;
        }
        soundTimer = swapTime;
    }
}
