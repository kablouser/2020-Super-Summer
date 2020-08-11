using UnityEngine;

[CreateAssetMenu(menuName = "Animation Timestamps")]
public class Timestamps : ScriptableObject
{
    [SerializeField]
    [ContextMenuItem("Convert frames to time", "ConvertFramesToTime")]
    private float[] times = null;

    public WaitForSeconds GetNextWait(int index)
    {
        if (0 <= index && index < times.Length)
        {
            float previous = index == 0 ? 0 : times[index - 1];
            return new WaitForSeconds(times[index] - previous);
        }
        else
        {
            Debug.LogError("Timestamp missing index "+index, this);
            return null;
        }
    }

    private void ConvertFramesToTime()
    {
        for (int i = 0; i < times.Length; i++)
            times[i] = times[i] / AnimationConstants.FramesPerSecond;
    }
}
