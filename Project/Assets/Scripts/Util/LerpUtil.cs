using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Lerp2D
{
    // Ease와 관련된 유틸리티 함수들 입니다.
    // https://easings.net/ko
    // https://gist.github.com/cjddmut/d789b9eb78216998e95c

    public static float Linear(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        return Mathf.Lerp(start, end, value);
    }

    public static float Spring(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
        return start + (end - start) * value;
    }
    public static Vector2 Spring(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = Spring(start.x, end.x, value);
        result.y = Spring(start.y, end.y, value);
        return result;
    }

    public static float EaseInQuad(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        return end * value * value + start;
    }
    public static Vector2 EaseInQuad(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInQuad(start.x, end.x, value);
        result.y = EaseInQuad(start.y, end.y, value);
        return result;
    }

    public static float EaseOutQuad(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        return -end * value * (value - 2) + start;
    }
    public static Vector2 EaseOutQuad(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseOutQuad(start.x, end.x, value);
        result.y = EaseOutQuad(start.y, end.y, value);
        return result;
    }

    public static float EaseInOutQuad(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value + start;
        value--;
        return -end * 0.5f * (value * (value - 2) - 1) + start;
    }
    public static Vector2 EaseInOutQuad(Vector2 start, Vector2 end, float value)
    {
        value = Mathf.Clamp01(value);
        Vector2 result = new Vector2();
        result.x = EaseInOutQuad(start.x, end.x, value);
        result.y = EaseInOutQuad(start.y, end.y, value);
        return result;
    }

    public static float EaseInCubic(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        return end * value * value * value + start;
    }
    public static Vector2 EaseInCubic(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInCubic(start.x, end.x, value);
        result.y = EaseInCubic(start.y, end.y, value);
        return result;
    }

    public static float EaseOutCubic(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value--;
        end -= start;
        return end * (value * value * value + 1) + start;
    }
    public static Vector2 EaseOutCubic(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseOutCubic(start.x, end.x, value);
        result.y = EaseOutCubic(start.y, end.y, value);
        return result;
    }

    public static float EaseInOutCubic(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value + start;
        value -= 2;
        return end * 0.5f * (value * value * value + 2) + start;
    }
    public static Vector2 EaseInOutCubic(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInOutCubic(start.x, end.x, value);
        result.y = EaseInOutCubic(start.y, end.y, value);
        return result;
    }

    public static float EaseInQuart(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        return end * value * value * value * value + start;
    }
    public static Vector2 EaseInQuart(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInOutCubic(start.x, end.x, value);
        result.y = EaseInOutCubic(start.y, end.y, value);
        return result;
    }

    public static float EaseOutQuart(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value--;
        end -= start;
        return -end * (value * value * value * value - 1) + start;
    }
    public static Vector2 EaseOutQuart(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseOutQuart(start.x, end.x, value);
        result.y = EaseOutQuart(start.y, end.y, value);
        return result;
    }

    public static float EaseInOutQuart(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value * value + start;
        value -= 2;
        return -end * 0.5f * (value * value * value * value - 2) + start;
    }
    public static Vector2 EaseInOutQuart(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInOutQuart(start.x, end.x, value);
        result.y = EaseInOutQuart(start.y, end.y, value);
        return result;
    }

    public static float EaseInQuint(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        return end * value * value * value * value * value + start;
    }
    public static Vector2 EaseInQuint(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInQuint(start.x, end.x, value);
        result.y = EaseInQuint(start.y, end.y, value);
        return result;
    }

    public static float EaseOutQuint(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value--;
        end -= start;
        return end * (value * value * value * value * value + 1) + start;
    }
    public static Vector2 EaseOutQuint(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseOutQuint(start.x, end.x, value);
        result.y = EaseOutQuint(start.y, end.y, value);
        return result;
    }

    public static float EaseInOutQuint(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value * value * value + start;
        value -= 2;
        return end * 0.5f * (value * value * value * value * value + 2) + start;
    }
    public static Vector2 EaseInOutQuint(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInOutQuint(start.x, end.x, value);
        result.y = EaseInOutQuint(start.y, end.y, value);
        return result;
    }

    public static float EaseInSine(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        return -end * Mathf.Cos(value * (Mathf.PI * 0.5f)) + end + start;
    }
    public static Vector2 EaseInSine(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInSine(start.x, end.x, value);
        result.y = EaseInSine(start.y, end.y, value);
        return result;
    }

    public static float EaseOutSine(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        return end * Mathf.Sin(value * (Mathf.PI * 0.5f)) + start;
    }
    public static Vector2 EaseOutSine(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseOutSine(start.x, end.x, value);
        result.y = EaseOutSine(start.y, end.y, value);
        return result;
    }

    public static float EaseInOutSine(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        return -end * 0.5f * (Mathf.Cos(Mathf.PI * value) - 1) + start;
    }
    public static Vector2 EaseInOutSine(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInOutSine(start.x, end.x, value);
        result.y = EaseInOutSine(start.y, end.y, value);
        return result;
    }

    public static float EaseInExpo(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        return end * Mathf.Pow(2, 10 * (value - 1)) + start;
    }
    public static Vector2 EaseInExpo(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInExpo(start.x, end.x, value);
        result.y = EaseInExpo(start.y, end.y, value);
        return result;
    }

    public static float EaseOutExpo(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        return end * (-Mathf.Pow(2, -10 * value) + 1) + start;
    }
    public static Vector2 EaseOutExpo(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseOutExpo(start.x, end.x, value);
        result.y = EaseOutExpo(start.y, end.y, value);
        return result;
    }

    public static float EaseInOutExpo(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + start;
        value--;
        return end * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
    }
    public static Vector2 EaseInOutExpo(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInOutExpo(start.x, end.x, value);
        result.y = EaseInOutExpo(start.y, end.y, value);
        return result;
    }

    public static float EaseInCirc(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
    }
    public static Vector2 EaseInCirc(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInCirc(start.x, end.x, value);
        result.y = EaseInCirc(start.y, end.y, value);
        return result;
    }

    public static float EaseOutCirc(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value--;
        end -= start;
        return end * Mathf.Sqrt(1 - value * value) + start;
    }
    public static Vector2 EaseOutCirc(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseOutCirc(start.x, end.x, value);
        result.y = EaseOutCirc(start.y, end.y, value);
        return result;
    }

    public static float EaseInOutCirc(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value /= .5f;
        end -= start;
        if (value < 1) return -end * 0.5f * (Mathf.Sqrt(1 - value * value) - 1) + start;
        value -= 2;
        return end * 0.5f * (Mathf.Sqrt(1 - value * value) + 1) + start;
    }
    public static Vector2 EaseInOutCirc(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInOutCirc(start.x, end.x, value);
        result.y = EaseInOutCirc(start.y, end.y, value);
        return result;
    }

    public static float EaseInBounce(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        float d = 1f;
        return end - EaseOutBounce(0, end, d - value) + start;
    }
    public static Vector2 EaseInBounce(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInBounce(start.x, end.x, value);
        result.y = EaseInBounce(start.y, end.y, value);
        return result;
    }

    public static float EaseOutBounce(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value /= 1f;
        end -= start;
        if (value < (1 / 2.75f))
        {
            return end * (7.5625f * value * value) + start;
        }
        else if (value < (2 / 2.75f))
        {
            value -= (1.5f / 2.75f);
            return end * (7.5625f * (value) * value + .75f) + start;
        }
        else if (value < (2.5 / 2.75))
        {
            value -= (2.25f / 2.75f);
            return end * (7.5625f * (value) * value + .9375f) + start;
        }
        else
        {
            value -= (2.625f / 2.75f);
            return end * (7.5625f * (value) * value + .984375f) + start;
        }
    }
    public static Vector2 EaseOutBounce(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseOutBounce(start.x, end.x, value);
        result.y = EaseOutBounce(start.y, end.y, value);
        return result;
    }

    public static float EaseInOutBounce(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        float d = 1f;
        if (value < d * 0.5f) return EaseInBounce(0, end, value * 2) * 0.5f + start;
        else return EaseOutBounce(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
    }
    public static Vector2 EaseInOutBounce(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInOutBounce(start.x, end.x, value);
        result.y = EaseInOutBounce(start.y, end.y, value);
        return result;
    }

    public static float EaseInBack(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;
        value /= 1;
        float s = 1.70158f;
        return end * (value) * value * ((s + 1) * value - s) + start;
    }
    public static Vector2 EaseInBack(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInBack(start.x, end.x, value);
        result.y = EaseInBack(start.y, end.y, value);
        return result;
    }

    public static float EaseOutBack(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        float s = 1.70158f;
        end -= start;
        value = (value) - 1;
        return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
    }
    public static Vector2 EaseOutBack(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseOutBack(start.x, end.x, value);
        result.y = EaseOutBack(start.y, end.y, value);
        return result;
    }

    public static float EaseInOutBack(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        float s = 1.70158f;
        end -= start;
        value /= .5f;
        if ((value) < 1)
        {
            s *= (1.525f);
            return end * 0.5f * (value * value * (((s) + 1) * value - s)) + start;
        }
        value -= 2;
        s *= (1.525f);
        return end * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + start;
    }
    public static Vector2 EaseInOutBack(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInOutBack(start.x, end.x, value);
        result.y = EaseInOutBack(start.y, end.y, value);
        return result;
    }

    public static float EaseInElastic(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d) == 1) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return -(a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
    }
    public static Vector2 EaseInElastic(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInElastic(start.x, end.x, value);
        result.y = EaseInElastic(start.y, end.y, value);
        return result;
    }

    public static float EaseOutElastic(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d) == 1) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p * 0.25f;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
    }
    public static Vector2 EaseOutElastic(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseOutElastic(start.x, end.x, value);
        result.y = EaseOutElastic(start.y, end.y, value);
        return result;
    }

    public static float EaseInOutElastic(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d * 0.5f) == 2) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
        return a * Mathf.Pow(2, -10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
    }
    public static Vector2 EaseInOutElastic(Vector2 start, Vector2 end, float value)
    {
        Vector2 result = new Vector2();
        result.x = EaseInOutElastic(start.x, end.x, value);
        result.y = EaseInOutElastic(start.y, end.y, value);
        return result;
    }
}
