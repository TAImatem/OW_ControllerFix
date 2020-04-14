using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using PadEZ;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace XBoxControllerFix
{

	class GamePad_PC_XB1_Fix
	{
		public static void SetUp_Postfix(GamePad_PC_XB1 __instance, ref uint ___m_xb1Index, ref XboxControllerFix.XINPUT_State ___m_padState)
		{
			uint chck = ___m_xb1Index;
			for (uint num = 0u; num < 4u; num += 1u)
			{
				if (XboxControllerFix.XInputGetState(num, out ___m_padState) == 0u && !XboxControllerFix.occInds.Contains(num))
				{
					___m_xb1Index = num;
				}
			}
			XboxControllerFix.occInds.Add(___m_xb1Index);
			if (chck != ___m_xb1Index)
				ModConsole.Instance.WriteLine($"patch fixed index from {chck} to {___m_xb1Index}");
		}

		public static void Destr_Prefix(GamePad_PC_XB1 __instance, ref uint ___m_xb1Index)
		{
			XboxControllerFix.occInds.Remove(___m_xb1Index);
		}
	}


	class PadManager_Fix
	{
		public static bool Prefix(ThrusterModel __instance, ref GamePad __result, int padIndex, string name)
		{
			GamePad result = null;
			if (name != null)
			{
				if (name == "Controller (Xbox One For Windows)" || name == "Xbox One Wireless Controller" || name == "Bluetooth XINPUT compatible input device")
				{
					result = new GamePad_PC_XB1(padIndex, GamePad.PadType.XB1);
				}
				else if (name.ToUpper().Contains("XBOX") || name.ToUpper().Contains("XINPUT"))
				{
					result = new GamePad_PC_XB1(padIndex, GamePad.PadType.X360);
				}
			}
			if (result != null)
			{
				ModConsole.Instance.WriteLine($"patched controller type: #{padIndex} \"{name}\"; resulting type = {result.GetPadType()}");
				__result = result;
				return false;
			}
			else
			{
				ModConsole.Instance.WriteLine($"normal controller type: #{padIndex} \"{name}\"");
				return true;
			}
		}
	}

	public class XboxControllerFix : ModBehaviour
	{
		public override void Configure(IModConfig config)
		{
			ModHelper.HarmonyHelper.AddPostfix<PadEZ.GamePad_PC_XB1>("SetUp", typeof(GamePad_PC_XB1_Fix), "SetUp_Postfix");
			ModHelper.HarmonyHelper.AddPrefix<PadEZ.GamePad_PC_XB1>("Destroy", typeof(GamePad_PC_XB1_Fix), "Destr_Prefix");
			ModHelper.HarmonyHelper.AddPrefix<PadEZ.PadManager>("CreateController", typeof(PadManager_Fix), "Prefix");
			ModHelper.Console.WriteLine("Controller Fix patched in!");
			base.Configure(config);
		}
		private void Start()
		{
			ModHelper.Console.WriteLine("Controller Fix ready!");
		}
		public static HashSet<uint> occInds = new HashSet<uint>();

		[DllImport("XINPUT9_1_0.DLL")]
		public static extern uint XInputGetState(uint userIndex, out XboxControllerFix.XINPUT_State state);
		public struct XINPUT_State
		{
			public uint PacketNumber;
			public ushort Buttons;
			public byte LeftTrigger;
			public byte RightTrigger;
			public short ThumbLX;
			public short ThumbLY;
			public short ThumbRX;
			public short ThumbRY;
		}
	}

}
