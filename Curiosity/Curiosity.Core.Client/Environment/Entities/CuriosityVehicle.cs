﻿using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using System;

namespace Curiosity.Core.Client.Environment.Entities
{
    class CuriosityVehicle
    {
        private Vehicle _vehicle;
        private int _soundId = -1;
        private bool _sirenState;
        private string _tone;

        internal DateTime LastSeen;

        internal CuriosityVehicle(Vehicle vehicle)
        {
            _vehicle = vehicle;
            _vehicle.IsSirenSilent = true;
            LastSeen = DateTime.UtcNow;
        }

        internal void SetLightsState(bool state)
        {
            _vehicle.State.Set("light:setup", state, false);
            _vehicle.IsSirenActive = state;
        }

        internal void SetSirenTone(string tone)
        {
            _vehicle.State.Set("siren:lastSound", tone, false);
            _tone = tone;
        }

        internal void SetSirenToneState(bool sirenState)
        {
            _sirenState = sirenState;

            if (_sirenState)
            {
                if (_soundId == -1)
                {
                    if (!Audio.HasSoundFinished(_soundId)) return;
                    _soundId = Audio.PlaySoundFromEntity(_vehicle, _tone);
                    Logger.Debug($"Started sound with ID {_soundId} from {_vehicle.NetworkId}");
                }
            }
            else
            {
                Logger.Debug($"Stopping sound with ID {_soundId} from {_vehicle.NetworkId}");

                Audio.StopSound(_soundId);
                Audio.ReleaseSound(_soundId);

                _soundId = -1;
            }
        }
    }
}
