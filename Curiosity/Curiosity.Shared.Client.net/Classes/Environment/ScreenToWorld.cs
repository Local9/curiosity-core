using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Shared.Client.net.Classes.Environment
{
    public static class ScreenToWorld
    {
        public static RaycastResult Screen2World(IntersectOptions flags, Entity ignore)
        {
            int cursorX = 0, cursorY = 0;
            API.GetNuiCursorPosition(ref cursorX, ref cursorY);

            Vector3 camPos = API.GetGameplayCamCoord();
            Vector2 processedCoords = ProcessCoordinates(cursorX, cursorY);
            Vector3 targetVector = S2W(camPos, processedCoords);

            // Screen.ShowSubtitle($"{cursorX} - {cursorY}\n{camPos}\n{processedCoords}\n{targetVector}");

            Vector3 direction = SubVector(targetVector, camPos);
            Vector3 from = AddVector(camPos, MulNumber(direction, 0.05));
            Vector3 to = AddVector(camPos, MulNumber(direction, 300));

            Screen.ShowSubtitle($"{direction}\n{from}\n{to}");

            return World.Raycast(from, to, flags, ignore);
        }

        private static Vector3 S2W(Vector3 camPos, Vector2 processedCoords)
        {
            Vector3 camRot = API.GetGameplayCamRot(0);
            Vector3 camForward = RotationToDirection(camRot);
            Vector3 rotUp = AddVector(camRot, new Vector3(10f, 0f, 0f));
            Vector3 rotDown = AddVector(camRot, new Vector3(-10f, 0f, 0f));
            Vector3 rotLeft = AddVector(camRot, new Vector3(0f, 0f, -10f));
            Vector3 rotRight = AddVector(camRot, new Vector3(0f, 0f, 10f));

            Vector3 camRight = SubVector(RotationToDirection(rotRight), RotationToDirection(rotLeft));
            Vector3 camUp = SubVector(RotationToDirection(rotUp), RotationToDirection(rotDown));

            float rollRad = (float)-RadFromDeg(camRot.Y);

            Vector3 camRightRoll = SubVector(MulNumber(camRight, Math.Cos(rollRad)), MulNumber(camUp, Math.Sin(rollRad)));
            Vector3 camUpRoll = AddVector(MulNumber(camRight, Math.Sin(rollRad)), MulNumber(camUp, Math.Cos(rollRad)));

            Vector3 point3d = AddVector(
                AddVector(AddVector(camPos, MulNumber(camForward, 10.0)), camRightRoll),
                camUpRoll
            );

            Vector2 point2d = W2S(point3d);

            if (point2d.IsZero)
            {
                return AddVector(camPos, MulNumber(camForward, 10.0));
            }

            Vector3 point3dZero = AddVector(camPos, MulNumber(camForward, 10.0));
            Vector2 point2dZero = W2S(point3dZero);

            double eps = 0.001;

            if (Math.Abs(point2d.X - point2dZero.X) < eps || Math.Abs(point2d.Y - point2dZero.Y) < eps)
            {
                return AddVector(camPos, MulNumber(camForward, 10.0));
            }

            float scaleX = (processedCoords.X - point2dZero.X) / (point2d.X - point2dZero.X);
            float scaleY = (processedCoords.Y - point2dZero.Y) / (point2d.Y - point2dZero.Y);
            Vector3 point3Dret = AddVector(
                AddVector(
                    AddVector(camPos, MulNumber(camForward, 10.0)),
                    MulNumber(camRightRoll, scaleX)
                ),
                MulNumber(camUpRoll, scaleY)
            );

            return point3Dret;
        }

        private static Vector2 W2S(Vector3 point3d)
        {
            float screenX = 0, screenY = 0;
            if (!API.GetScreenCoordFromWorldCoord(point3d.X, point3d.Y, point3d.Z, ref screenX, ref screenY))
            {
                return Vector2.Zero;
            }
            return new Vector2(screenX, screenY);
        }

        private static Vector3 MulNumber(Vector3 v, double num)
        {
            Vector3 result = new Vector3();
            result.X = (float)(v.X * num);
            result.Y = (float)(v.Y * num);
            result.Z = (float)(v.Z * num);
            return result;
        }

        private static Vector3 SubVector(Vector3 v1, Vector3 v2)
        {
            Vector3 result = new Vector3();
            result.X = v1.X - v2.X;
            result.Y = v1.Y - v2.Y;
            result.Z = v1.Z - v2.Z;
            return result;
        }

        private static Vector3 AddVector(Vector3 v1, Vector3 v2)
        {
            Vector3 result = new Vector3();
            result.X = v1.X + v2.X;
            result.Y = v1.Y + v2.Y;
            result.Z = v1.Z + v2.Z;
            return result;
        }

        private static Vector3 RotationToDirection(Vector3 camRot)
        {
            float z = (float)RadFromDeg(camRot.Z);
            float x = (float)RadFromDeg(camRot.X);
            var num = Math.Abs(Math.Cos(x));
            
            Vector3 result = new Vector3();
            result.X = (float)(-Math.Sin(z) * num);
            result.Y = (float)(Math.Cos(z) * num);
            result.Z = (float)Math.Sin(x);
            return result;
        }

        private static Vector2 ProcessCoordinates(int x, int y)
        {
            int screenX = 0, screenY = 0;
            API.GetScreenActiveResolution(ref screenX, ref screenY);

            double relativeX = 1 - (((x / (float)screenX) * 1.0) * 2);
            double relativeY = 1 - (((y / (float)screenY) * 1.0) * 2);

            if (relativeX > 0.0)
            {
                relativeX = -relativeX;
            } else
            {
                relativeX = Math.Abs(relativeX);
            }

            if (relativeY > 0.0)
            {
                relativeY = -relativeY;
            }
            else
            {
                relativeY = Math.Abs(relativeY);
            }

            // Screen.ShowSubtitle($"{x} - {y}\n{screenX} - {screenY}\n{relativeX} - {relativeY}");

            return new Vector2((float)relativeX, (float)relativeY);
        }

        static public double RadFromDeg(double degrees)
        {
            return degrees * Math.PI / 180.0f;
        }

        static public double DegFromRad(double radians)
        {
            return radians * 180.0 / Math.PI;
        }
    }
}
