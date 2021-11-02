using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Police.Client.Diagnostics;
using Curiosity.Police.Client.Extensions;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Police.Client.Environment.Entities.Models
{
    public class AnimationQueue
    {
        private int Entity { get { return Game.PlayerPed.Handle; } }
        private List<Animation> Queue { get; } = new List<Animation>();
        private bool PlayingQueue { get; set; }
        private Animation LastPlayed { get; set; }

        public AnimationQueue(int entity)
        {
            // Entity = entity;
        }

        public async Task<AnimationQueue> PlayDirectInQueue(AnimationBuilder builder)
        {
            Queue.Add(builder.Animation);
            PlayQueue();

            while (!builder.Animation.HasBeenPlayed)
            {
                await BaseScript.Delay(10);
            }

            return this;
        }

        public AnimationQueue AddToQueue(AnimationBuilder builder)
        {
            Queue.Add(builder.Animation);

            return this;
        }

        public void PlayQueue()
        {
            if (!PlayingQueue) BeginQueue();
        }

        private async void BeginQueue()
        {
            Animation next;

            while ((next = Queue.FirstOrDefault()) != null)
            {
                PlayingQueue = true;

                var entity = CitizenFX.Core.Entity.FromHandle(Entity);

                if (entity == null || !entity.Exists())
                {
                    Logger.Info($"[AnimationQueue] Could not find entity #{Entity}");

                    return;
                }

                var ped = (Ped)entity;

                if (next.Position != null)
                {
                    if (API.NetworkGetEntityIsNetworked(Entity))
                    {
                        API.NetworkRequestControlOfEntity(Entity);

                        while (!API.NetworkHasControlOfEntity(Entity))
                        {
                            await BaseScript.Delay(100);
                        }
                    }

                    if (!next.PositionInstant)
                    {
                        API.TaskGoStraightToCoord(Entity, next.Position.X, next.Position.Y, next.Position.Z,
                            1f,
                            -1,
                            next.Position.H, 0f);

                        var time = Date.Timestamp;

                        while ((next.Position.Distance(ped.Position.ToPosition()) > 0.3 ||
                                Math.Abs(next.Position.H - ped.Heading) > 5) && next.PositionTimeout == -1 ||
                               time + next.PositionTimeout > Date.Timestamp)
                        {
                            await BaseScript.Delay(10);
                        }
                    }
                    else
                    {
                        API.SetEntityCoords(Entity, next.Position.X, next.Position.Y, next.Position.Z, false, false,
                            false, false);
                    }

                    ped.Task.ClearAllImmediately();

                    API.SetEntityHeading(Entity, next.Position.H);
                }

                next.OnAnimStart?.Invoke();

                if (next.ClearBefore) ped.Task.ClearAllImmediately();
                if (next.Scenario != null)
                {
                    API.TaskStartScenarioInPlace(ped.Handle, next.Scenario, 0, true);
                }
                else
                {
                    if (next.Offset == null && next.StartTime.Equals(0f))
                        await ped.Task.PlayAnimation(next.Group, next.AnimationId, 8f, -8f, -1, next.Flags, 0);
                    else
                    {
                        var offset = new Position();

                        if (next.Offset != null)
                        {
                            var position = next.Position ?? ped.Position.ToPosition();

                            position.H = ped.Heading;

                            offset = position.Add(next.Offset);
                        }

                        API.RequestAnimDict(next.Group);

                        while (!API.HasAnimDictLoaded(next.Group))
                        {
                            await BaseScript.Delay(10);
                        }

                        API.TaskPlayAnimAdvanced(ped.Handle, next.Group, next.AnimationId, offset.X, offset.Y, offset.Z,
                            0f, 0f, offset.H, 8f, -8f, -1, (int)next.Flags, next.StartTime, 0, 0);
                        API.RemoveAnimDict(next.Group);
                    }

                    if (next.TaskDuration == -1)
                    {
                        await BaseScript.Delay(100);

                        while (API.IsEntityPlayingAnim(Entity, next.Group, next.AnimationId, 3))
                        {
                            await BaseScript.Delay(1);
                        }
                    }
                    else
                    {
                        await BaseScript.Delay(next.TaskDuration);
                    }
                }

                next.HasBeenPlayed = true;

                if (LastPlayed?.Group != next.Group)
                {
                    API.RemoveAnimDict(next.Group);
                }

                Queue.RemoveAt(0);

                LastPlayed = next;
            }

            if (LastPlayed != null) API.RemoveAnimDict(LastPlayed.Group);

            PlayingQueue = false;
        }
    }

    public class Animation
    {
        public string Group { get; set; }
        public string AnimationId { get; set; }
        public string Scenario { get; set; }
        public AnimationFlags Flags { get; set; } = AnimationFlags.None;
        public Position Position { get; set; }
        public Position Offset { get; set; }
        public float StartTime { get; set; }
        public int PositionTimeout { get; set; }
        public bool PositionInstant { get; set; }
        public int TaskDuration { get; set; } = -1;
        public bool ClearBefore { get; set; }
        public bool HasBeenPlayed { get; set; }
        public Action OnAnimStart { get; set; }
    }

    public class AnimationBuilder
    {
        public Animation Animation { get; set; } = new Animation();

        public AnimationBuilder Select(string group, string animation)
        {
            Animation.Group = group;
            Animation.AnimationId = animation;

            return this;
        }

        public AnimationBuilder Select(string scenario)
        {
            Animation.Scenario = scenario;

            return this;
        }

        public AnimationBuilder WithFlags(AnimationFlags flags)
        {
            Animation.Flags = flags;

            return this;
        }

        public AnimationBuilder AtPosition(Position position, int timeout = -1, bool instant = false)
        {
            Animation.Position = position;
            Animation.PositionTimeout = timeout;
            Animation.PositionInstant = instant;

            return this;
        }

        public AnimationBuilder WithOffset(Position position)
        {
            Animation.Offset = position;

            return this;
        }

        public AnimationBuilder AtTime(float time)
        {
            Animation.StartTime = time;

            return this;
        }

        public AnimationBuilder ClearBefore()
        {
            Animation.ClearBefore = true;

            return this;
        }

        public AnimationBuilder WithTaskDuration(int duration)
        {
            Animation.TaskDuration = duration;

            return this;
        }

        public AnimationBuilder SkipTask()
        {
            Animation.TaskDuration = 0;

            return this;
        }

        public AnimationBuilder On(Action action)
        {
            Animation.OnAnimStart = action;

            return this;
        }

        public async Task TaskWhenPlayed()
        {
            while (!Animation.HasBeenPlayed)
            {
                await BaseScript.Delay(10);
            }
        }
    }
}