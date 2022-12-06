/**
Unity Generic Serial Controller

Author: Brandon Matthews
2018
 */

/**
Holds constants such as \r\n etc. so they can be configured easily per platform etc.
 */
namespace Connectivity
{   
    [System.Serializable]
    public enum WaitFor
    {
        EndOfFrame,
        DataAvailable,
    }

    public class Constants
    {
        public static int CARRIAGE_RETURN = 13;
        public static int LINE_FEED = 10;
    }
}
