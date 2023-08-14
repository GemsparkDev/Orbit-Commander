using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main
{
    public static class SoundManager
    {
        public static List<SoundEffectInstance> sounds = new();

        public static void Initialize()
        {
            SetAllSounds(false);
            sounds = new();
        }
        public static void PlaySound(SoundEffectInstance _soundEffectInstance)
        {
            if (!sounds.Contains(_soundEffectInstance))
            {
                sounds.Add(_soundEffectInstance);
            }
            _soundEffectInstance.Play();
        }
        public static void AddSound(SoundEffectInstance _soundEffectInstance)
        {
            if (!sounds.Contains(_soundEffectInstance))
            {
                sounds.Add(_soundEffectInstance);
            }
            _soundEffectInstance.Play();
            _soundEffectInstance.Pause();
        }
        public static void PlaySound(SoundEffect _sound, Vector2 _playLocation)
        {
            Vector2 listenerLocation = EntityManager.player.position;
            float distance = MathF.Sqrt(MathF.Pow(_playLocation.X - listenerLocation.X, 2) + MathF.Pow(_playLocation.Y - listenerLocation.Y, 2));
            float volume = -(distance / 1000) + 1;
            //float pan = -(listenerLocation.X - playLocation.X) / (screenSize.X);
            //Monogame issue, audio cannot pan smoothly
            if (volume < 0) { volume = 0; }
            SoundEffectInstance soundInstance = _sound.CreateInstance();
            soundInstance.Volume = volume;
            sounds.Add(soundInstance);
            soundInstance.Play();
        }
        public static void PlayGlobalSound(SoundEffect sound)
        {
            sound.Play(1, 0, 0);
        }
        public static void PauseSound(SoundEffectInstance _soundEffectInstance)
        {
            if (sounds.Contains(_soundEffectInstance))
            {
                _soundEffectInstance.Pause();
            }
        }
        public static void SetAllSounds(bool _playingSounds)
        {
            if(_playingSounds == true)
            {
                foreach (SoundEffectInstance soundEffect in sounds)
                {
                    soundEffect.Resume();
                }
            }
            else
            {
                foreach (SoundEffectInstance soundEffect in sounds)
                {
                    soundEffect.Pause();
                }
            }
        }
        public static void Update()
        {
            sounds = sounds.Where(sound => !(sound.State == SoundState.Stopped)).ToList();
        }
    }
}
