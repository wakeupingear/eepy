using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Eepy
{
    public enum Direction
    {
        Right = 0,
        Up = 1,
        Left = 2,
        Down = 3,
        MAX = 4
    }

    public static class Util
    {
        public static Direction zeroDirection = Direction.Right;
        public static Direction[] directions = { Direction.Right, Direction.Up, Direction.Left, Direction.Down };
        public static Vector2Int[] directionVectors = { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };
        public static float[] directionAngles = { 0f, 90f, 180f, -90f };

        public static float Damp(float a, float b, float lambda, float dt)
        {
            return Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
        }

        public static Vector3 Damp(Vector3 a, Vector3 b, float lambda, float dt)
        {
            return Vector3.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
        }

        public static Quaternion Damp(Quaternion a, Quaternion b, float lambda, float dt)
        {
            return Quaternion.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
        }

        public static Direction VecToDir(Vector2Int vec)
        {
            if (vec.x > 0)
            {
                return Direction.Right;
            }
            if (vec.x < 0)
            {
                return Direction.Left;
            }
            if (vec.y > 0)
            {
                return Direction.Up;
            }
            if (vec.y < 0)
            {
                return Direction.Down;
            }
            return Direction.Up;
        }

        public static Vector2Int DirToVec(Direction dir)
        {
            return directionVectors[(int)dir];
        }

        public static float DirToAngle(Direction dir)
        {
            return directionAngles[(int)dir];
        }

        public static Direction RotateCounterClockwise(Direction dir)
        {
            return (Direction)(((int)dir + 1) % 4);
        }

        public static Direction Reverse(Direction dir)
        {
            return (Direction)(((int)dir + 2) % 4);
        }

        public static Direction RotateClockwise(Direction dir)
        {
            return (Direction)(((int)dir + 3) % 4);
        }

        // Rotates directionA by directionB
        // Interprets directionB as a rotation from Direction.Right
        public static Direction RotateByDirection(Direction directionA, Direction directionB)
        {
            return (Direction)(((int)directionA + (int)directionB) % 4);
        }

        public static Direction HorizontalDirection(Vector2Int vec)
        {
            return vec.x >= 0 ? Direction.Right : Direction.Left;
        }

        public static Direction VerticalDirection(Vector2Int vec)
        {
            return vec.y >= 0 ? Direction.Up : Direction.Down;
        }

        /*
        // TODO: uncomment
        public static bool IsInputActionDirectional(InputAction input)
        {
            switch (input)
            {
                case InputAction.Right:
                case InputAction.Up:
                case InputAction.Left:
                case InputAction.Down:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsInputActionGameplayRelated(InputAction input)
        {
            switch (input)
            {
                case InputAction.Right:
                case InputAction.Up:
                case InputAction.Left:
                case InputAction.Down:
                case InputAction.Wait:
                case InputAction.Interact:
                    return true;
                default:
                    return false;
            }
        }

        public static Direction GetDirectionFromInputAction(InputAction input)
        {
            switch (input)
            {
                case InputAction.Right:
                    return Direction.Right;
                case InputAction.Up:
                    return Direction.Up;
                case InputAction.Left:
                    return Direction.Left;
                case InputAction.Down:
                    return Direction.Down;
                default:
                    Debug.Log($"Tried to turn the input {input} into a direction. This is invalid.");
                    return Direction.Right;
            }
        }
        */

        public static Vector2 IntToFloat(Vector2Int inInt)
        {
            return new Vector2(inInt.x, inInt.y);
        }

        public static Vector2Int PairwiseMax(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }

        public static Vector2Int PairwiseMin(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
        }

        public static Vector2 PairwiseMax(Vector2 a, Vector2 b)
        {
            return new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }

        public static Vector2 PairwiseMin(Vector2 a, Vector2 b)
        {
            return new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
        }

        public static Vector3 PairwiseMax(Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
        }

        public static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        public static int AbsMax(Vector2Int a)
        {
            return Mathf.Max(Mathf.Abs(a.x), Mathf.Abs(a.y));
        }

        public static bool IsDirectionHorizontal(Direction dir)
        {
            switch (dir)
            {
                case Direction.Left:
                case Direction.Right:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsDirectionPositive(Direction dir)
        {
            switch (dir)
            {
                case Direction.Right:
                case Direction.Up:
                    return true;
                default:
                    return false;
            }
        }

        public static Vector2Int RotatePositionCounterClockwise(Vector2Int inPos)
        {
            return new Vector2Int(-inPos.y, inPos.x);
        }

        public static int GetVectorComponent(Vector2Int vec, bool getX)
        {
            return getX ? vec.x : vec.y;
        }

        public static Vector2Int MakeVectorComponent(int value, bool isX)
        {
            return isX ? new Vector2Int(value, 0) : new Vector2Int(0, value);
        }

        public static Vector3 Flatten(Vector3 inVector)
        {
            return new Vector3(inVector.x, 0.0f, inVector.z);
        }

        public static bool IsVectorUnitDirection(Vector2Int inVec)
        {
            return (Mathf.Abs(inVec.x) == 1) ^ (Mathf.Abs(inVec.y) == 1);
        }

        public static Vector3 DirectionToWorldVector(Direction direction)
        {
            switch (direction)
            {
                case Direction.Right:
                    return Vector3.right;
                case Direction.Up:
                    return Vector3.forward;
                case Direction.Left:
                    return Vector3.left;
                case Direction.Down:
                    return Vector3.back;
                default:
                    return Vector3.zero;
            }
        }

        public static Vector2 Floor(Vector2 val)
        {
            return new Vector2(Mathf.Floor(val.x), Mathf.Floor(val.y));
        }

        public static Vector3 Floor(Vector3 val)
        {
            return new Vector3(Mathf.Floor(val.x), Mathf.Floor(val.y), Mathf.Floor(val.z));
        }

        public static Vector4 Floor(Vector4 val)
        {
            return new Vector4(Mathf.Floor(val.x), Mathf.Floor(val.y), Mathf.Floor(val.z), Mathf.Floor(val.w));
        }

        public static Vector2 PairwiseMultiply(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x * b.x,
                a.y * b.y
            );
        }

        public static Vector3 PairwiseMultiply(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.x * b.x,
                a.y * b.y,
                a.z * b.z
            );
        }

        public static Vector4 PairwiseMultiply(Vector4 a, Vector4 b)
        {
            return new Vector4(
                a.x * b.x,
                a.y * b.y,
                a.z * b.z,
                a.w * b.w
            );
        }

        public static int Step(float a, float b)
        {
            return (a <= b) ? 1 : 0;
        }

        public static float Frac(float val)
        {
            return val - Mathf.Floor(val);
        }

        public static Vector3 Frac(Vector3 val)
        {
            return new Vector3(Frac(val.x), Frac(val.y), Frac(val.z));
        }

        public static Vector3 PairwiseAbs(Vector3 val)
        {
            return new Vector3(Mathf.Abs(val.x), Mathf.Abs(val.y), Mathf.Abs(val.z));
        }

        public static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
        {
            float currentAlpha = canvasGroup.alpha;
            float totalDelta = to - from;

            float startT = Mathf.Clamp01((currentAlpha - from) / totalDelta);
            float elapsed = startT * duration;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(from, to, t);

                yield return null;
            }

            canvasGroup.alpha = to;
        }

        public static IEnumerator FadeImage(Image image, float from, float to, float duration)
        {
            float currentAlpha = image.color.a;
            float totalDelta = to - from;

            float startT = Mathf.Clamp01((currentAlpha - from) / totalDelta);
            float elapsed = startT * duration;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(from, to, t));

                yield return null;
            }

            image.color = new Color(image.color.r, image.color.g, image.color.b, to);
        }

        public static Color PerceptualColorLerp(Color a, Color b, float t)
        {
            const float epsilon = 0.0001f;

            float red = Mathf.Exp(Mathf.Lerp(Mathf.Log(Mathf.Max(a.r, epsilon)), Mathf.Log(Mathf.Max(b.r, epsilon)), t));
            float green = Mathf.Exp(Mathf.Lerp(Mathf.Log(Mathf.Max(a.g, epsilon)), Mathf.Log(Mathf.Max(b.g, epsilon)), t));
            float blue = Mathf.Exp(Mathf.Lerp(Mathf.Log(Mathf.Max(a.b, epsilon)), Mathf.Log(Mathf.Max(b.b, epsilon)), t));
            float alpha = Mathf.Lerp(a.a, b.a, t);

            return new Color(red, green, blue, alpha);
        }

        public static void ClampFramerateToDisplay()
        {
            int targetFrameRate = Mathf.Clamp((int)(Screen.currentResolution.refreshRateRatio.numerator / Screen.currentResolution.refreshRateRatio.denominator), 59, 144);
            Application.targetFrameRate = targetFrameRate;
        }

        public static bool InputCodeIsKeyboard(int code)
        {
            return code >= 0;
        }

        public static string FormatInputCode(int code)
        {
            if (InputCodeIsKeyboard(code))
            {
                return ((KeyCode)code).ToString();
            }
            else
            {
                return ((ControllerCode)code).ToString();
            }
        }

        public static bool IsAxisPressed(InputManager.ControllerAxisBinding axis, float threshold = 0.0f)
        {
            float axisValue = Input.GetAxis(axis.axis);
            if (axis.dir == InputManager.ControllerAxisDir.Negative)
            {
                axisValue *= -1;
            }
            return axisValue > threshold;
        }


        public enum LogLevel
        {
            None = 0,
            Error = 1,
            Warning = 2,
            Info = 3
        }
        public static Util.LogLevel logLevel = Util.LogLevel.Warning;

        public static void LogError(object message)
        {
            if (logLevel <= LogLevel.Error)
            {
                Debug.LogError(message);
            }
        }

        public static void LogWarning(object message)
        {
            if (logLevel <= LogLevel.Warning)
            {
                Debug.LogWarning(message);
            }
        }

        public static void Log(object message)
        {
            if (logLevel <= LogLevel.Info)
            {
                Debug.Log(message);
            }
        }

        public static bool AnyInputActionListDown(List<InputAction> actions, bool allowWhenInputManagerDisabled = false)
        {
            foreach (InputAction action in actions)
            {
                if (InputManager.GetKeyDown(action, allowWhenInputManagerDisabled))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool AnyKeyCodeListDown(List<KeyCode> keys)
        {
            foreach (KeyCode key in keys)
            {
                if (Input.GetKeyDown(key))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool AnyInputActionList(List<InputAction> actions, bool allowWhenInputManagerDisabled = false)
        {
            foreach (InputAction action in actions)
            {
                if (InputManager.GetKey(action, allowWhenInputManagerDisabled))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool AnyKeyCodeList(List<KeyCode> keys)
        {
            foreach (KeyCode key in keys)
            {
                if (Input.GetKey(key))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool AnyInputActionListUp(List<InputAction> actions, bool allowWhenInputManagerDisabled = false)
        {
            foreach (InputAction action in actions)
            {
                if (InputManager.GetKeyUp(action, allowWhenInputManagerDisabled))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool AnyKeyCodeListUp(List<KeyCode> keys)
        {
            foreach (KeyCode key in keys)
            {
                if (Input.GetKeyUp(key))
                {
                    return true;
                }
            }

            return false;
        }

        // GetComponentsInChildren, except we stop recursing down if we don't find any components of type T at the current level
        // Always recurse down 2 levels before potentially stopping
        public static List<T> GetComponentsInChildrenConditional<T>(Transform root) where T : Component
        {
            List<T> result = new List<T>();
            Traverse(root, result, 0);

            return result;
        }

        private static void Traverse<T>(Transform current, List<T> result, int depth = 0) where T : Component
        {
            T[] componentsHere = current.GetComponents<T>();

            if (componentsHere.Length > 0 || depth < 2)
            {
                result.AddRange(componentsHere);

                foreach (Transform child in current)
                {
                    Traverse(child, result, depth + 1);
                }
            }
        }

        public static void SetTextUnderline(TMP_Text text, bool underline)
        {
            if (text != null)
            {
                if (!underline)
                {
                    text.fontStyle &= ~TMPro.FontStyles.Underline;
                }
                else
                {
                    text.fontStyle |= TMPro.FontStyles.Underline;
                }
            }
        }

        public static void SetTextUnderline(List<TMP_Text> texts, bool underline)
        {
            foreach (var text in texts)
            {
                SetTextUnderline(text, underline);
                SetTextUnderline(text, underline);
            }
        }
    }
}