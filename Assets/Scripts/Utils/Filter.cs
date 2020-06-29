using System.Collections.Generic;

namespace SignalProcessing
{
    public struct Filter
    {
        private readonly uint epsilon, deadzone;

        private (uint input, uint? output) pprev, prev;

        private bool jumpedFromZero, midDrop;

        public Filter(uint epsilon, uint deadzone)
        {
            pprev = (0, null);
            prev = (0, null);

            midDrop = false;
            jumpedFromZero = false;

            this.epsilon = epsilon;
            this.deadzone = deadzone;
        }

        public uint? Push(uint rawIn)
        {
            bool inDeadzone = rawIn < deadzone;
            bool jumpingFromZero = prev.output == 0 && deadzone + epsilon <= rawIn;
            bool steepDrop = 2.5f * epsilon + rawIn <= prev.input;
            bool steepJump = 1.5f * epsilon + prev.input <= rawIn;
            bool inNeighborhood = isNeighbor(rawIn, prev.output) && isNeighbor(rawIn, pprev.output);
            midDrop = midDrop && !inDeadzone && !inNeighborhood;

            uint? currentOutput;
            // if in deadzone or mid-drop -> 0
            // if just jumping from 0 -> 0
            // if did jump from 0 -> can't jump again, rawIn
            // if jumping quickly -> preempt rise, rawIn + 0.75 * last rise
            // if dropping quickly -> prev = pprev, set mid-drop, 0
            // if near prev and pprev inputs -> prev + pprev / 2
            // else raw

            if (inDeadzone || midDrop)
            {
                currentOutput = 0;
            }
            else if (jumpingFromZero)
            {
                currentOutput = jumpedFromZero ? rawIn : 0;
                jumpedFromZero = !jumpedFromZero;
            }
            else if (steepJump)
            {
                currentOutput = rawIn + (3 * (rawIn - prev.input)) / 4;
            }
            else if (steepDrop)
            {
                currentOutput = 0;
                prev.output = pprev.output;
                midDrop = true;
            }
            else if (inNeighborhood)
            {
                currentOutput = (pprev.output + prev.output) / 2;
            }
            else
            {
                currentOutput = rawIn;
            }

            uint? output = pprev.output;

            pprev = prev;
            prev = (rawIn, currentOutput);

            return output;
        }


        // get the partially fitlered values currently in storage
        // warning: these values may change before they are returned by using Push
        public void DumpPartials(ref List<uint> list)
        {
            list.OptionalAdd(pprev.output);
            list.OptionalAdd(prev.output);
        }

        // value is within radius epsilon of rawin
        private bool isNeighbor(uint rawin, uint? value)
            => value.HasValue && rawin - epsilon <= value && value <= rawin + epsilon;
    }

    public static partial class Utils
    {
        // returns a filtered list of the provided data, constructing a new filter from the optional arguments
        public static List<uint> BatchFilter(IEnumerable<uint> data, uint epsilon = 2, uint deadzone = 8)
        {
            var filter = new Filter(epsilon, deadzone);
            return BatchFilter(ref filter, data);
        }

        // returns a filtered list of the data, modifying the provided filter in the process
        public static List<uint> BatchFilter(ref Filter filter, IEnumerable<uint> data)
        {
            var list = new List<uint>();

            foreach (var item in data)
            {
                list.OptionalAdd(filter.Push(item));
            }
            filter.DumpPartials(ref list);

            return list;
        }
    }
}