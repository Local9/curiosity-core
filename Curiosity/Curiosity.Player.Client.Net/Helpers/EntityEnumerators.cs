using CitizenFX.Core.Native;
using System.Collections;
using System.Collections.Generic;

namespace Curiosity.Client.net.Helpers
{
    public class VehicleList : IEnumerable<int>
    {
        public IEnumerator<int> GetEnumerator()
        {
            OutputArgument OutArgEntity = new OutputArgument();
            int handle = Function.Call<int>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_FIRST_VEHICLE")), OutArgEntity);
            yield return OutArgEntity.GetResult<int>();
            while (Function.Call<bool>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_NEXT_VEHICLE")), handle, OutArgEntity))
            {
                yield return OutArgEntity.GetResult<int>();
            }
            Function.Call((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("END_FIND_VEHICLE")), handle);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class PedList : IEnumerable<int>
    {
        public IEnumerator<int> GetEnumerator()
        {
            OutputArgument OutArgEntity = new OutputArgument();
            int handle = Function.Call<int>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_FIRST_PED")), OutArgEntity);
            yield return OutArgEntity.GetResult<int>();
            while (Function.Call<bool>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_NEXT_PED")), handle, OutArgEntity))
            {
                yield return OutArgEntity.GetResult<int>();
            }
            Function.Call((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("END_FIND_PED")), handle);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ObjectList : IEnumerable<int>
    {
        public IEnumerator<int> GetEnumerator()
        {
            OutputArgument OutArgEntity = new OutputArgument();
            int handle = Function.Call<int>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_FIRST_OBJECT")), OutArgEntity);
            yield return OutArgEntity.GetResult<int>();
            while (Function.Call<bool>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_NEXT_OBJECT")), handle, OutArgEntity))
            {
                yield return OutArgEntity.GetResult<int>();
            }
            Function.Call((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("END_FIND_OBJECT")), handle);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
