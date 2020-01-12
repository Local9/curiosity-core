using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Curiosity.System.Client.Extensions;
using Curiosity.System.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.System.Client.Environment.Entities.Models
{
    public class CameraViewmodelQueue
    {
        public List<CameraViewmodel> Queue { get; set; } = new List<CameraViewmodel>();
        public bool PlayingQueue { get; set; }
        public CameraViewmodel LastViewed { get; set; }

        public async Task View(CameraBuilder builder)
        {
            Queue.Add(builder.Camera);
            Continue();

            while (!builder.Camera.HasBeenViewed)
            {
                await BaseScript.Delay(10);
            }
        }

        private void Continue()
        {
            if (!PlayingQueue) Next();
        }

        private async void Next()
        {
            CameraViewmodel next;

            while ((next = Queue.FirstOrDefault()) != null)
            {
                if (!PlayingQueue) PlayingQueue = true;
                if (next.Static != null)
                {
                    var camera = World.CreateCamera(next.Static.AsVector(), next.Static.Rotations(), next.FieldOfView);

                    if (next.Shake != null)
                    {
                        camera.Shake(next.Shake.Item1, next.Shake.Item2);
                    }

                    if (next.MotionBlur > 0)
                    {
                        camera.MotionBlurStrength = next.MotionBlur;
                    }

                    camera.IsActive = true;

                    API.RenderScriptCams(true, false, 0, true, false);
                }
                else
                {
                    var cameras = new Camera[2];

                    foreach (var entry in next.Interpolation.Take(2))
                    {
                        for (var i = 0; i < 2; i++)
                        {
                            var camera = World.CreateCamera(i == 0 ? entry.Item1.AsVector() : entry.Item2.AsVector(),
                                i == 0 ? entry.Item1.Rotations() : entry.Item2.Rotations(),
                                next.FieldOfView);

                            if (next.Shake != null)
                            {
                                camera.Shake(next.Shake.Item1, next.Shake.Item2);
                            }

                            if (next.MotionBlur > 0)
                            {
                                camera.MotionBlurStrength = next.MotionBlur;
                            }

                            cameras[i] = camera;
                        }
                    }

                    cameras[0].InterpTo(cameras[1], next.Interpolation.First().Item3, true, true);

                    API.RenderScriptCams(true, false, 0, true, false);

                    if (!next.SkipTask)
                    {
                        await BaseScript.Delay(next.Interpolation.First().Item3);
                    }
                }

                next.HasBeenViewed = true;

                Queue.RemoveAt(0);

                LastViewed = next;
            }

            PlayingQueue = false;
        }

        public void Reset()
        {
            World.DestroyAllCameras();

            API.RenderScriptCams(false, true, 0, true, false);
        }
    }

    public class CameraViewmodel
    {
        public float FieldOfView { get; set; } = GameplayCamera.FieldOfView;
        public float MotionBlur { get; set; }
        public RotatablePosition Static { get; set; }

        public List<Tuple<RotatablePosition, RotatablePosition, int>> Interpolation { get; set; } =
            new List<Tuple<RotatablePosition, RotatablePosition, int>>();

        public Tuple<CameraShake, float> Shake { get; set; }

        public bool SkipTask { get; set; }
        public bool HasBeenViewed { get; set; }
    }

    public class CameraBuilder
    {
        public CameraViewmodel Camera { get; set; } = new CameraViewmodel();

        public CameraBuilder WithStatic(RotatablePosition position)
        {
            Camera.Static = position;

            return this;
        }

        public CameraBuilder WithFieldOfView(float fov)
        {
            Camera.FieldOfView = fov;

            return this;
        }

        public CameraBuilder WithMotionBlur(float motionBlur)
        {
            Camera.MotionBlur = motionBlur;

            return this;
        }

        public CameraBuilder WithInterpolation(RotatablePosition beginning, RotatablePosition endpoint, int duration)
        {
            Camera.Interpolation.Add(
                new Tuple<RotatablePosition, RotatablePosition, int>(beginning, endpoint, duration));

            return this;
        }

        public CameraBuilder WithShake(CameraShake shake, float intensity)
        {
            Camera.Shake = new Tuple<CameraShake, float>(shake, intensity);

            return this;
        }

        public CameraBuilder SkipTask()
        {
            Camera.SkipTask = true;

            return this;
        }
    }
}