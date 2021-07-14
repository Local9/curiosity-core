using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Control = CitizenFX.Core.Control;
using Font = CitizenFX.Core.UI.Font;
using CitizenFX.Core.UI;
using System.Drawing;
using System.Threading.Tasks;

namespace NativeUI
{
	#region Delegates
	public enum Keys
	{
		//
		// Summary:
		//     The bitmask to extract modifiers from a key value.
		Modifiers = -65536,
		//
		// Summary:
		//     No key pressed.
		None = 0,
		//
		// Summary:
		//     The left mouse button.
		LButton = 1,
		//
		// Summary:
		//     The right mouse button.
		RButton = 2,
		//
		// Summary:
		//     The CANCEL key.
		Cancel = 3,
		//
		// Summary:
		//     The middle mouse button (three-button mouse).
		MButton = 4,
		//
		// Summary:
		//     The first x mouse button (five-button mouse).
		XButton1 = 5,
		//
		// Summary:
		//     The second x mouse button (five-button mouse).
		XButton2 = 6,
		//
		// Summary:
		//     The BACKSPACE key.
		Back = 8,
		//
		// Summary:
		//     The TAB key.
		Tab = 9,
		//
		// Summary:
		//     The LINEFEED key.
		LineFeed = 10,
		//
		// Summary:
		//     The CLEAR key.
		Clear = 12,
		//
		// Summary:
		//     The RETURN key.
		Return = 13,
		//
		// Summary:
		//     The ENTER key.
		Enter = 13,
		//
		// Summary:
		//     The SHIFT key.
		ShiftKey = 16,
		//
		// Summary:
		//     The CTRL key.
		ControlKey = 17,
		//
		// Summary:
		//     The ALT key.
		Menu = 18,
		//
		// Summary:
		//     The PAUSE key.
		Pause = 19,
		//
		// Summary:
		//     The CAPS LOCK key.
		Capital = 20,
		//
		// Summary:
		//     The CAPS LOCK key.
		CapsLock = 20,
		//
		// Summary:
		//     The IME Kana mode key.
		KanaMode = 21,
		//
		// Summary:
		//     The IME Hanguel mode key. (maintained for compatibility; use HangulMode)
		HanguelMode = 21,
		//
		// Summary:
		//     The IME Hangul mode key.
		HangulMode = 21,
		//
		// Summary:
		//     The IME Junja mode key.
		JunjaMode = 23,
		//
		// Summary:
		//     The IME final mode key.
		FinalMode = 24,
		//
		// Summary:
		//     The IME Hanja mode key.
		HanjaMode = 25,
		//
		// Summary:
		//     The IME Kanji mode key.
		KanjiMode = 25,
		//
		// Summary:
		//     The ESC key.
		Escape = 27,
		//
		// Summary:
		//     The IME convert key.
		IMEConvert = 28,
		//
		// Summary:
		//     The IME nonconvert key.
		IMENonconvert = 29,
		//
		// Summary:
		//     The IME accept key, replaces System.Windows.Forms.Keys.IMEAceept.
		IMEAccept = 30,
		//
		// Summary:
		//     The IME accept key. Obsolete, use System.Windows.Forms.Keys.IMEAccept instead.
		IMEAceept = 30,
		//
		// Summary:
		//     The IME mode change key.
		IMEModeChange = 31,
		//
		// Summary:
		//     The SPACEBAR key.
		Space = 32,
		//
		// Summary:
		//     The PAGE UP key.
		Prior = 33,
		//
		// Summary:
		//     The PAGE UP key.
		PageUp = 33,
		//
		// Summary:
		//     The PAGE DOWN key.
		Next = 34,
		//
		// Summary:
		//     The PAGE DOWN key.
		PageDown = 34,
		//
		// Summary:
		//     The END key.
		End = 35,
		//
		// Summary:
		//     The HOME key.
		Home = 36,
		//
		// Summary:
		//     The LEFT ARROW key.
		Left = 37,
		//
		// Summary:
		//     The UP ARROW key.
		Up = 38,
		//
		// Summary:
		//     The RIGHT ARROW key.
		Right = 39,
		//
		// Summary:
		//     The DOWN ARROW key.
		Down = 40,
		//
		// Summary:
		//     The SELECT key.
		Select = 41,
		//
		// Summary:
		//     The PRINT key.
		Print = 42,
		//
		// Summary:
		//     The EXECUTE key.
		Execute = 43,
		//
		// Summary:
		//     The PRINT SCREEN key.
		Snapshot = 44,
		//
		// Summary:
		//     The PRINT SCREEN key.
		PrintScreen = 44,
		//
		// Summary:
		//     The INS key.
		Insert = 45,
		//
		// Summary:
		//     The DEL key.
		Delete = 46,
		//
		// Summary:
		//     The HELP key.
		Help = 47,
		//
		// Summary:
		//     The 0 key.
		D0 = 48,
		//
		// Summary:
		//     The 1 key.
		D1 = 49,
		//
		// Summary:
		//     The 2 key.
		D2 = 50,
		//
		// Summary:
		//     The 3 key.
		D3 = 51,
		//
		// Summary:
		//     The 4 key.
		D4 = 52,
		//
		// Summary:
		//     The 5 key.
		D5 = 53,
		//
		// Summary:
		//     The 6 key.
		D6 = 54,
		//
		// Summary:
		//     The 7 key.
		D7 = 55,
		//
		// Summary:
		//     The 8 key.
		D8 = 56,
		//
		// Summary:
		//     The 9 key.
		D9 = 57,
		//
		// Summary:
		//     The A key.
		A = 65,
		//
		// Summary:
		//     The B key.
		B = 66,
		//
		// Summary:
		//     The C key.
		C = 67,
		//
		// Summary:
		//     The D key.
		D = 68,
		//
		// Summary:
		//     The E key.
		E = 69,
		//
		// Summary:
		//     The F key.
		F = 70,
		//
		// Summary:
		//     The G key.
		G = 71,
		//
		// Summary:
		//     The H key.
		H = 72,
		//
		// Summary:
		//     The I key.
		I = 73,
		//
		// Summary:
		//     The J key.
		J = 74,
		//
		// Summary:
		//     The K key.
		K = 75,
		//
		// Summary:
		//     The L key.
		L = 76,
		//
		// Summary:
		//     The M key.
		M = 77,
		//
		// Summary:
		//     The N key.
		N = 78,
		//
		// Summary:
		//     The O key.
		O = 79,
		//
		// Summary:
		//     The P key.
		P = 80,
		//
		// Summary:
		//     The Q key.
		Q = 81,
		//
		// Summary:
		//     The R key.
		R = 82,
		//
		// Summary:
		//     The S key.
		S = 83,
		//
		// Summary:
		//     The T key.
		T = 84,
		//
		// Summary:
		//     The U key.
		U = 85,
		//
		// Summary:
		//     The V key.
		V = 86,
		//
		// Summary:
		//     The W key.
		W = 87,
		//
		// Summary:
		//     The X key.
		X = 88,
		//
		// Summary:
		//     The Y key.
		Y = 89,
		//
		// Summary:
		//     The Z key.
		Z = 90,
		//
		// Summary:
		//     The left Windows logo key (Microsoft Natural Keyboard).
		LWin = 91,
		//
		// Summary:
		//     The right Windows logo key (Microsoft Natural Keyboard).
		RWin = 92,
		//
		// Summary:
		//     The application key (Microsoft Natural Keyboard).
		Apps = 93,
		//
		// Summary:
		//     The computer sleep key.
		Sleep = 95,
		//
		// Summary:
		//     The 0 key on the numeric keypad.
		NumPad0 = 96,
		//
		// Summary:
		//     The 1 key on the numeric keypad.
		NumPad1 = 97,
		//
		// Summary:
		//     The 2 key on the numeric keypad.
		NumPad2 = 98,
		//
		// Summary:
		//     The 3 key on the numeric keypad.
		NumPad3 = 99,
		//
		// Summary:
		//     The 4 key on the numeric keypad.
		NumPad4 = 100,
		//
		// Summary:
		//     The 5 key on the numeric keypad.
		NumPad5 = 101,
		//
		// Summary:
		//     The 6 key on the numeric keypad.
		NumPad6 = 102,
		//
		// Summary:
		//     The 7 key on the numeric keypad.
		NumPad7 = 103,
		//
		// Summary:
		//     The 8 key on the numeric keypad.
		NumPad8 = 104,
		//
		// Summary:
		//     The 9 key on the numeric keypad.
		NumPad9 = 105,
		//
		// Summary:
		//     The multiply key.
		Multiply = 106,
		//
		// Summary:
		//     The add key.
		Add = 107,
		//
		// Summary:
		//     The separator key.
		Separator = 108,
		//
		// Summary:
		//     The subtract key.
		Subtract = 109,
		//
		// Summary:
		//     The decimal key.
		Decimal = 110,
		//
		// Summary:
		//     The divide key.
		Divide = 111,
		//
		// Summary:
		//     The F1 key.
		F1 = 112,
		//
		// Summary:
		//     The F2 key.
		F2 = 113,
		//
		// Summary:
		//     The F3 key.
		F3 = 114,
		//
		// Summary:
		//     The F4 key.
		F4 = 115,
		//
		// Summary:
		//     The F5 key.
		F5 = 116,
		//
		// Summary:
		//     The F6 key.
		F6 = 117,
		//
		// Summary:
		//     The F7 key.
		F7 = 118,
		//
		// Summary:
		//     The F8 key.
		F8 = 119,
		//
		// Summary:
		//     The F9 key.
		F9 = 120,
		//
		// Summary:
		//     The F10 key.
		F10 = 121,
		//
		// Summary:
		//     The F11 key.
		F11 = 122,
		//
		// Summary:
		//     The F12 key.
		F12 = 123,
		//
		// Summary:
		//     The F13 key.
		F13 = 124,
		//
		// Summary:
		//     The F14 key.
		F14 = 125,
		//
		// Summary:
		//     The F15 key.
		F15 = 126,
		//
		// Summary:
		//     The F16 key.
		F16 = 127,
		//
		// Summary:
		//     The F17 key.
		F17 = 128,
		//
		// Summary:
		//     The F18 key.
		F18 = 129,
		//
		// Summary:
		//     The F19 key.
		F19 = 130,
		//
		// Summary:
		//     The F20 key.
		F20 = 131,
		//
		// Summary:
		//     The F21 key.
		F21 = 132,
		//
		// Summary:
		//     The F22 key.
		F22 = 133,
		//
		// Summary:
		//     The F23 key.
		F23 = 134,
		//
		// Summary:
		//     The F24 key.
		F24 = 135,
		//
		// Summary:
		//     The NUM LOCK key.
		NumLock = 144,
		//
		// Summary:
		//     The SCROLL LOCK key.
		Scroll = 145,
		//
		// Summary:
		//     The left SHIFT key.
		LShiftKey = 160,
		//
		// Summary:
		//     The right SHIFT key.
		RShiftKey = 161,
		//
		// Summary:
		//     The left CTRL key.
		LControlKey = 162,
		//
		// Summary:
		//     The right CTRL key.
		RControlKey = 163,
		//
		// Summary:
		//     The left ALT key.
		LMenu = 164,
		//
		// Summary:
		//     The right ALT key.
		RMenu = 165,
		//
		// Summary:
		//     The browser back key (Windows 2000 or later).
		BrowserBack = 166,
		//
		// Summary:
		//     The browser forward key (Windows 2000 or later).
		BrowserForward = 167,
		//
		// Summary:
		//     The browser refresh key (Windows 2000 or later).
		BrowserRefresh = 168,
		//
		// Summary:
		//     The browser stop key (Windows 2000 or later).
		BrowserStop = 169,
		//
		// Summary:
		//     The browser search key (Windows 2000 or later).
		BrowserSearch = 170,
		//
		// Summary:
		//     The browser favorites key (Windows 2000 or later).
		BrowserFavorites = 171,
		//
		// Summary:
		//     The browser home key (Windows 2000 or later).
		BrowserHome = 172,
		//
		// Summary:
		//     The volume mute key (Windows 2000 or later).
		VolumeMute = 173,
		//
		// Summary:
		//     The volume down key (Windows 2000 or later).
		VolumeDown = 174,
		//
		// Summary:
		//     The volume up key (Windows 2000 or later).
		VolumeUp = 175,
		//
		// Summary:
		//     The media next track key (Windows 2000 or later).
		MediaNextTrack = 176,
		//
		// Summary:
		//     The media previous track key (Windows 2000 or later).
		MediaPreviousTrack = 177,
		//
		// Summary:
		//     The media Stop key (Windows 2000 or later).
		MediaStop = 178,
		//
		// Summary:
		//     The media play pause key (Windows 2000 or later).
		MediaPlayPause = 179,
		//
		// Summary:
		//     The launch mail key (Windows 2000 or later).
		LaunchMail = 180,
		//
		// Summary:
		//     The select media key (Windows 2000 or later).
		SelectMedia = 181,
		//
		// Summary:
		//     The start application one key (Windows 2000 or later).
		LaunchApplication1 = 182,
		//
		// Summary:
		//     The start application two key (Windows 2000 or later).
		LaunchApplication2 = 183,
		//
		// Summary:
		//     The OEM Semicolon key on a US standard keyboard (Windows 2000 or later).
		OemSemicolon = 186,
		//
		// Summary:
		//     The OEM 1 key.
		Oem1 = 186,
		//
		// Summary:
		//     The OEM plus key on any country/region keyboard (Windows 2000 or later).
		Oemplus = 187,
		//
		// Summary:
		//     The OEM comma key on any country/region keyboard (Windows 2000 or later).
		Oemcomma = 188,
		//
		// Summary:
		//     The OEM minus key on any country/region keyboard (Windows 2000 or later).
		OemMinus = 189,
		//
		// Summary:
		//     The OEM period key on any country/region keyboard (Windows 2000 or later).
		OemPeriod = 190,
		//
		// Summary:
		//     The OEM question mark key on a US standard keyboard (Windows 2000 or later).
		OemQuestion = 191,
		//
		// Summary:
		//     The OEM 2 key.
		Oem2 = 191,
		//
		// Summary:
		//     The OEM tilde key on a US standard keyboard (Windows 2000 or later).
		Oemtilde = 192,
		//
		// Summary:
		//     The OEM 3 key.
		Oem3 = 192,
		//
		// Summary:
		//     The OEM open bracket key on a US standard keyboard (Windows 2000 or later).
		OemOpenBrackets = 219,
		//
		// Summary:
		//     The OEM 4 key.
		Oem4 = 219,
		//
		// Summary:
		//     The OEM pipe key on a US standard keyboard (Windows 2000 or later).
		OemPipe = 220,
		//
		// Summary:
		//     The OEM 5 key.
		Oem5 = 220,
		//
		// Summary:
		//     The OEM close bracket key on a US standard keyboard (Windows 2000 or later).
		OemCloseBrackets = 221,
		//
		// Summary:
		//     The OEM 6 key.
		Oem6 = 221,
		//
		// Summary:
		//     The OEM singled/double quote key on a US standard keyboard (Windows 2000 or later).
		OemQuotes = 222,
		//
		// Summary:
		//     The OEM 7 key.
		Oem7 = 222,
		//
		// Summary:
		//     The OEM 8 key.
		Oem8 = 223,
		//
		// Summary:
		//     The OEM angle bracket or backslash key on the RT 102 key keyboard (Windows 2000
		//     or later).
		OemBackslash = 226,
		//
		// Summary:
		//     The OEM 102 key.
		Oem102 = 226,
		//
		// Summary:
		//     The PROCESS KEY key.
		ProcessKey = 229,
		//
		// Summary:
		//     Used to pass Unicode characters as if they were keystrokes. The Packet key value
		//     is the low word of a 32-bit virtual-key value used for non-keyboard input methods.
		Packet = 231,
		//
		// Summary:
		//     The ATTN key.
		Attn = 246,
		//
		// Summary:
		//     The CRSEL key.
		Crsel = 247,
		//
		// Summary:
		//     The EXSEL key.
		Exsel = 248,
		//
		// Summary:
		//     The ERASE EOF key.
		EraseEof = 249,
		//
		// Summary:
		//     The PLAY key.
		Play = 250,
		//
		// Summary:
		//     The ZOOM key.
		Zoom = 251,
		//
		// Summary:
		//     A constant reserved for future use.
		NoName = 252,
		//
		// Summary:
		//     The PA1 key.
		Pa1 = 253,
		//
		// Summary:
		//     The CLEAR key.
		OemClear = 254,
		//
		// Summary:
		//     The bitmask to extract a key code from a key value.
		KeyCode = 65535,
		//
		// Summary:
		//     The SHIFT modifier key.
		Shift = 65536,
		//
		// Summary:
		//     The CTRL modifier key.
		Control = 131072,
		//
		// Summary:
		//     The ALT modifier key.
		Alt = 262144
	}

	public enum MenuState
	{
		Opened,
		Closed,
		ChangeForward,
		ChangeBackward
	}

	public delegate void IndexChangedEvent(UIMenu sender, int newIndex);

	public delegate void ListChangedEvent(UIMenu sender, UIMenuListItem listItem, int newIndex);

	public delegate void SliderChangedEvent(UIMenu sender, UIMenuSliderItem listItem, int newIndex);

	public delegate void ListSelectedEvent(UIMenu sender, UIMenuListItem listItem, int newIndex);

	public delegate void CheckboxChangeEvent(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked);

	public delegate void ItemSelectEvent(UIMenu sender, UIMenuItem selectedItem, int index);

	public delegate void ItemActivatedEvent(UIMenu sender, UIMenuItem selectedItem);

	public delegate void ItemCheckboxEvent(UIMenuCheckboxItem sender, bool Checked);

	public delegate void ItemListEvent(UIMenuListItem sender, int newIndex);

	public delegate void ItemSliderEvent(UIMenuSliderItem sender, int newIndex);

	public delegate void ItemSliderProgressEvent(UIMenuSliderProgressItem sender, int newIndex);

	public delegate void OnProgressChanged(UIMenu menu, UIMenuProgressItem item, int newIndex);

	public delegate void OnProgressSelected(UIMenu menu, UIMenuProgressItem item, int newIndex);

	public delegate void ProgressSliderChangedEvent(UIMenu menu, UIMenuSliderProgressItem item, int newIndex);

	

	#endregion

	/// <summary>
	/// Base class for NativeUI. Calls the next events: OnIndexChange, OnListChanged, OnCheckboxChange, OnItemSelect, OnMenuClose, OnMenuchange.
	/// </summary>
	public class UIMenu
	{
		#region Private Fields
		private readonly Container _mainMenu;
		private readonly Sprite _background;

		private readonly UIResRectangle _descriptionBar;
		private readonly Sprite _descriptionRectangle;
		private readonly UIResText _descriptionText;
		private readonly UIResText _counterText;

		private int _activeItem = 1000;

		private bool _visible;
		private bool _buttonsEnabled = true;
		private bool _justOpened = true;
		private bool _itemsDirty = false;

		//Pagination
		private const int MaxItemsOnScreen = 9;
		private int _minItem;
		private int _maxItem = MaxItemsOnScreen;

		//Keys
		private readonly Dictionary<MenuControls, Tuple<List<Keys>, List<Tuple<Control, int>>>> _keyDictionary =
			new Dictionary<MenuControls, Tuple<List<Keys>, List<Tuple<Control, int>>>>();

		private readonly Sprite _upAndDownSprite;
		private readonly UIResRectangle _extraRectangleUp;
		private readonly UIResRectangle _extraRectangleDown;

		private readonly Scaleform _menuGlare;

		private readonly int _extraYOffset;

		private static readonly MenuControls[] _menuControls = Enum.GetValues(typeof(MenuControls)).Cast<MenuControls>().ToArray();

		private float PanelOffset = 0;
		internal MenuPool _poolcontainer;

		// Draw Variables
		private PointF Safe { get; set; }
		private SizeF BackgroundSize { get; set; }
		private SizeF DrawWidth { get; set; }
		private bool ReDraw = true;

		private bool Glare;

		internal readonly static string _selectTextLocalized = Game.GetGXTEntry("HUD_INPUT2");
		internal readonly static string _backTextLocalized = Game.GetGXTEntry("HUD_INPUT3");
		protected readonly SizeF Resolution = ScreenTools.ResolutionMaintainRatio;
		protected internal int _pressingTimer = 0;
		#endregion

		#region Public Fields

		public string AUDIO_LIBRARY = "HUD_FRONTEND_DEFAULT_SOUNDSET";

		public string AUDIO_UPDOWN = "NAV_UP_DOWN";
		public string AUDIO_LEFTRIGHT = "NAV_LEFT_RIGHT";
		public string AUDIO_SELECT = "SELECT";
		public string AUDIO_BACK = "BACK";
		public string AUDIO_ERROR = "ERROR";

		public List<UIMenuItem> MenuItems = new List<UIMenuItem>();

		public bool MouseEdgeEnabled = true;
		public bool ControlDisablingEnabled = true;
		public bool ResetCursorOnOpen = true;
		[Obsolete("The description is now formated automatically by the game.")]
		public bool FormatDescriptions = true;
		public bool MouseControlsEnabled = true;
		public bool ScaleWithSafezone = true;

		public PointF Offset { get; }

		public Sprite BannerSprite { get; private set; }
		public UIResRectangle BannerRectangle { get; private set; }
		public string BannerTexture { get; private set; }

		public List<UIMenuHeritageWindow> Windows = new List<UIMenuHeritageWindow>();

		public List<InstructionalButton> InstructionalButtons = new List<InstructionalButton>()
		{
			new InstructionalButton(Control.PhoneSelect, _selectTextLocalized),
			new InstructionalButton(Control.PhoneCancel, _backTextLocalized)
		};

		#endregion

		#region Events

		/// <summary>
		/// Called when user presses up or down, changing current selection.
		/// </summary>
		public event IndexChangedEvent OnIndexChange;

		/// <summary>
		/// Called when user presses left or right, changing a list position.
		/// </summary>
		public event ListChangedEvent OnListChange;

		/// <summary>
		/// Called when user selects a list item.
		/// </summary>
		public event ListSelectedEvent OnListSelect;

		/// <summary>
		/// Called when user presses left or right, changing a slider position.
		/// </summary>
		public event SliderChangedEvent OnSliderChange;

		/// <summary>
		/// Called when user presses left or right, changing a progress slider position.
		/// </summary>
		public event ProgressSliderChangedEvent OnProgressSliderChange;

		/// <summary>
		/// Called When user changes progress in a ProgressItem.
		/// </summary>
		public event OnProgressChanged OnProgressChange;

		/// <summary>
		/// Called when user either clicks on a ProgressItem.
		/// </summary>
		public event OnProgressSelected OnProgressSelect;

		/// <summary>
		/// Called when user presses enter on a checkbox item.
		/// </summary>
		public event CheckboxChangeEvent OnCheckboxChange;

		/// <summary>
		/// Called when user selects a simple item.
		/// </summary>
		public event ItemSelectEvent OnItemSelect;

		/// <summary>
		/// Called when user either opens or closes the main menu, clicks on a binded button, goes back to a parent menu.
		/// </summary>
		public event MenuStateChangeEvent OnMenuStateChanged;

		#endregion

		#region Constructors

		/// <summary>
		/// Basic Menu constructor.
		/// </summary>
		/// <param name="title">Title that appears on the big banner.</param>
		/// <param name="subtitle">Subtitle that appears in capital letters in a small black bar.</param>
		/// <param name="glare">Add menu Glare scaleform?.</param>
		public UIMenu(string title, string subtitle, bool glare = false) : this(title, subtitle, new PointF(0, 0), "commonmenu", "interaction_bgd", glare)
		{
		}


		/// <summary>
		/// Basic Menu constructor with an offset.
		/// </summary>
		/// <param name="title">Title that appears on the big banner.</param>
		/// <param name="subtitle">Subtitle that appears in capital letters in a small black bar. Set to "" if you dont want a subtitle.</param>
		/// <param name="offset">PointF object with X and Y data for offsets. Applied to all menu elements.</param>
		/// <param name="glare">Add menu Glare scaleform?.</param>
		public UIMenu(string title, string subtitle, PointF offset, bool glare = false) : this(title, subtitle, offset, "commonmenu", "interaction_bgd", glare)
		{
		}

		/// <summary>
		/// Initialise a menu with a custom texture banner.
		/// </summary>
		/// <param name="title">Title that appears on the big banner. Set to "" if you don't want a title.</param>
		/// <param name="subtitle">Subtitle that appears in capital letters in a small black bar. Set to "" if you dont want a subtitle.</param>
		/// <param name="offset">PointF object with X and Y data for offsets. Applied to all menu elements.</param>
		/// <param name="customBanner">Path to your custom texture.</param>
		/// <param name="glare">Add menu Glare scaleform?.</param>
		public UIMenu(string title, string subtitle, PointF offset, string customBanner) : this(title, subtitle, offset, "commonmenu", "interaction_bgd", false)
		{
			BannerTexture = customBanner;
		}


		/// <summary>
		/// Advanced Menu constructor that allows custom title banner.
		/// </summary>
		/// <param name="title">Title that appears on the big banner. Set to "" if you are using a custom banner.</param>
		/// <param name="subtitle">Subtitle that appears in capital letters in a small black bar.</param>
		/// <param name="offset">PointF object with X and Y data for offsets. Applied to all menu elements.</param>
		/// <param name="spriteLibrary">Sprite library name for the banner.</param>
		/// <param name="spriteName">Sprite name for the banner.</param>
		/// <param name="glare">Add menu Glare scaleform?.</param>
		public UIMenu(string title, string subtitle, PointF offset, string spriteLibrary, string spriteName, bool glare = false)
		{
			Offset = offset;
			Children = new Dictionary<UIMenuItem, UIMenu>();
			WidthOffset = 0;
			Glare = glare;
			_menuGlare = new Scaleform("mp_menu_glare");

			_mainMenu = new Container(new PointF(0, 0), new SizeF(700, 500), Color.FromArgb(0, 0, 0, 0));
			BannerSprite = new Sprite(spriteLibrary, spriteName, new PointF(0 + Offset.X, 0 + Offset.Y), new SizeF(431, 100));
			_mainMenu.Items.Add(Title = new UIResText(title, new PointF(215 + Offset.X, 13 + Offset.Y), 1.15f, Colors.White, Font.HouseScript, Alignment.Center));
			if (!String.IsNullOrWhiteSpace(subtitle))
			{
				_mainMenu.Items.Add(new UIResRectangle(new PointF(0 + offset.X, 100 + Offset.Y), new SizeF(431, 37), Colors.Black));
				_mainMenu.Items.Add(Subtitle = new UIResText(subtitle, new PointF(8 + Offset.X, 103 + Offset.Y), 0.35f, Colors.WhiteSmoke, 0, Alignment.Left));

				if (subtitle.StartsWith("~"))
				{
					CounterPretext = subtitle.Substring(0, 3);
				}
				_counterText = new UIResText("", new PointF(425 + Offset.X, 103 + Offset.Y), 0.35f, Colors.WhiteSmoke, 0, Alignment.Right);
				_extraYOffset = 30;
			}

			_upAndDownSprite = new Sprite("commonmenu", "shop_arrows_upanddown", new PointF(190 + Offset.X, 147 + 37 * (MaxItemsOnScreen + 1) + Offset.Y - 37 + _extraYOffset), new SizeF(50, 50));
			_extraRectangleUp = new UIResRectangle(new PointF(0 + Offset.X, 144 + 38 * (MaxItemsOnScreen + 1) + Offset.Y - 37 + _extraYOffset), new SizeF(431, 18), Color.FromArgb(200, 0, 0, 0));
			_extraRectangleDown = new UIResRectangle(new PointF(0 + Offset.X, 144 + 18 + 38 * (MaxItemsOnScreen + 1) + Offset.Y - 37 + _extraYOffset), new SizeF(431, 18), Color.FromArgb(200, 0, 0, 0));

			_descriptionBar = new UIResRectangle(new PointF(Offset.X, 123), new SizeF(431, 4), Colors.Black);
			_descriptionRectangle = new Sprite("commonmenu", "gradient_bgd", new PointF(Offset.X, 127), new SizeF(431, 30));
			_descriptionText = new UIResText("Description", new PointF(Offset.X + 5, 125), 0.35f, Color.FromArgb(255, 255, 255, 255), Font.ChaletLondon, Alignment.Left);

			_background = new Sprite("commonmenu", "gradient_bgd", new PointF(Offset.X, 144 + Offset.Y - 37 + _extraYOffset), new SizeF(290, 25));


			SetKey(MenuControls.Up, Control.PhoneUp);
			SetKey(MenuControls.Up, Control.CursorScrollUp);

			SetKey(MenuControls.Down, Control.PhoneDown);
			SetKey(MenuControls.Down, Control.CursorScrollDown);

			SetKey(MenuControls.Left, Control.PhoneLeft);
			SetKey(MenuControls.Right, Control.PhoneRight);
			SetKey(MenuControls.Select, Control.FrontendAccept);

			SetKey(MenuControls.Back, Control.PhoneCancel);
			SetKey(MenuControls.Back, Control.FrontendPause);
		}

		#endregion

		#region Static Methods
		/// <summary>
		/// Toggles the availability of the controls.
		/// It does not disable the basic movement and frontend controls.
		/// </summary>
		/// <param name="enable"></param>
		/// <param name="toggle">If we want to enable or disable the controls.</param>
		[Obsolete("Use Controls.Toggle instead.", true)]
		public static void DisEnableControls(bool toggle) => Controls.Toggle(toggle);

		/// <summary>
		/// Returns the 1080pixels-based screen resolution while mantaining current aspect ratio.
		/// </summary>
		[Obsolete("Use ScreenTools.ResolutionMaintainRatio instead.", true)]
		public static SizeF GetScreenResolutionMaintainRatio() => ScreenTools.ResolutionMaintainRatio;

		/// <summary>
		/// ScreenTools.ResolutionMaintainRatio for providing backwards compatibility.
		/// </summary>
		/// <returns></returns>
		[Obsolete("Use ScreenTools.ResolutionMaintainRatio instead.", true)]
		public static SizeF GetScreenResiolutionMantainRatio() => ScreenTools.ResolutionMaintainRatio;

		/// <summary>
		/// Chech whether the mouse is inside the specified rectangle.
		/// </summary>
		/// <param name="topLeft">Start point of the rectangle at the top left.</param>
		/// <param name="boxSize">size of your rectangle.</param>
		/// <returns>true if the mouse is inside of the specified bounds, false otherwise.</returns>
		[Obsolete("Use ScreenTools.IsMouseInBounds instead.", true)]
		public static bool IsMouseInBounds(Point topLeft, Size boxSize) => ScreenTools.IsMouseInBounds(topLeft, boxSize);

		/// <summary>
		/// Returns the safezone bounds in pixel, relative to the 1080pixel based system.
		/// </summary>
		[Obsolete("Use ScreenTools.SafezoneBounds instead.", true)]
		public static Point GetSafezoneBounds() => ScreenTools.SafezoneBounds;

		#endregion

		#region Public Methods
		/// <summary>
		/// Change the menu's width. The width is calculated as DefaultWidth + WidthOffset, so a width offset of 10 would enlarge the menu by 10 pixels.
		/// </summary>
		/// <param name="widthOffset">New width offset.</param>
		public void SetMenuWidthOffset(int widthOffset)
		{
			WidthOffset = widthOffset;
			BannerSprite .Size = new SizeF(431 + WidthOffset, 100);
			_mainMenu.Items[0].Position = new PointF((WidthOffset + Offset.X + 431) / 2, 20 + Offset.Y); // Title
			_counterText.Position = new PointF(425 + Offset.X + widthOffset, 110 + Offset.Y);
			if (_mainMenu.Items.Count >= 1)
			{
				UIResRectangle tmp = (UIResRectangle)_mainMenu.Items[1];
				tmp.Size = new SizeF(431 + WidthOffset, 37);
			}
			if (BannerRectangle != null)
			{
				BannerRectangle.Size = new SizeF(431 + WidthOffset, 100);
			}
		}

		/// <summary>
		/// Enable or disable the instructional buttons.
		/// </summary>
		/// <param name="disable"></param>
		public void DisableInstructionalButtons(bool disable)
		{
			InstructionalButtonsHandler.InstructionalButtons.Enabled = !disable;
		}

		/// <summary>
		/// Set the banner to your own Sprite object.
		/// </summary>
		/// <param name="spriteBanner">Sprite object. The position and size does not matter.</param>
		public void SetBannerType(Sprite spriteBanner)
		{
			BannerSprite  = spriteBanner;
			BannerSprite .Size = new SizeF(431 + WidthOffset, 100);
			BannerSprite .Position = new PointF(Offset.X, Offset.Y);
		}

		/// <summary>
		///  Set the banner to your own Rectangle.
		/// </summary>
		/// <param name="rectangle">UIResRectangle object. Position and size does not matter.</param>
		public void SetBannerType(UIResRectangle rectangle)
		{
			BannerSprite  = null;
			BannerRectangle = rectangle;
			BannerRectangle.Position = new PointF(Offset.X, Offset.Y);
			BannerRectangle.Size = new SizeF(431 + WidthOffset, 100);
		}

		/// <summary>
		/// Set the banner to your own custom texture. Set it to "" if you want to restore the banner.
		/// </summary>
		/// <param name="pathToCustomSprite">Path to your sprite image.</param>
		public void SetBannerType(string pathToCustomSprite)
		{
			BannerTexture = pathToCustomSprite;
		}

		/// <summary>
		/// Add an item to the menu.
		/// </summary>
		/// <param name="item">Item object to be added. Can be normal item, checkbox or list item.</param>
		public void AddItem(UIMenuItem item)
		{
			int selectedItem = CurrentSelection;

			item.Offset = Offset;
			item.Parent = this;
			item.Position((MenuItems.Count * 25) - 37 + _extraYOffset);
			MenuItems.Add(item);
			ReDraw = true;
			CurrentSelection = selectedItem;
		}

		/// <summary>
		/// Add a new Heritage Window to the Menu
		/// </summary>
		/// <param name="window"></param>
		public void AddWindow(UIMenuHeritageWindow window)
		{
			window.ParentMenu = this;
			window.Offset = Offset;
			Windows.Add(window);
			ReDraw = true;
		}

		/// <summary>
		/// Removes Windows at given index
		/// </summary>
		/// <param name="index"></param>
		public void RemoveWindowAt(int index)
		{
			Windows.RemoveAt(index);
			ReDraw = true;
		}

		/// <summary>
		/// If a Description is changed during some events after the menu as been opened this updates the description live
		/// </summary>
		public void UpdateDescription()
		{
			ReDraw = true;
		}

		/// <summary>
		/// Remove an item at index n.
		/// </summary>
		/// <param name="index">Index to remove the item at.</param>
		public void RemoveItemAt(int index)
		{
			int selectedItem = CurrentSelection;
			if (Size > MaxItemsOnScreen && _maxItem == Size - 1)
			{
				_maxItem--;
				_minItem--;
			}
			MenuItems.RemoveAt(index);
			ReDraw = true;
			CurrentSelection = selectedItem;
		}

		/// <summary>
		/// Reset the current selected item to 0. Use this after you add or remove items dynamically.
		/// </summary>
		public void RefreshIndex()
		{
			if (MenuItems.Count == 0)
			{
				_activeItem = 1000;
				_maxItem = MaxItemsOnScreen;
				_minItem = 0;
				return;
			}
			MenuItems[_activeItem % (MenuItems.Count)].Selected = false;
			_activeItem = 1000 - (1000 % MenuItems.Count);
			_maxItem = MaxItemsOnScreen;
			_minItem = 0;

			ReDraw = true;
		}

		/// <summary>
		/// Remove all items from the menu.
		/// </summary>
		public void Clear()
		{
			MenuItems.Clear();
			ReDraw = true;
		}

		/// <summary>
		/// Removes the items that matches the predicate.
		/// </summary>
		/// <param name="predicate">The function to use as the check.</param>
		public void Remove(Func<UIMenuItem, bool> predicate)
		{
			List<UIMenuItem> TempList = new List<UIMenuItem>(MenuItems);
			foreach (UIMenuItem item in TempList)
			{
				if (predicate(item))
				{
					MenuItems.Remove(item);
				}
			}
			ReDraw = true;
		}

		private float CalculateWindowHeight()
		{
			float height = 0;
			if (Windows.Count > 0)
				for (int i = 0; i < Windows.Count; i++)
					height += Windows[i].Background.Size.Height;
			return height;
		}

		private float CalculateItemHeight()
		{
			float ItemOffset = 0 + _mainMenu.Items[1].Position.Y - 37f;
			for (int i = 0; i < MenuItems.Count; i++)
				ItemOffset += MenuItems[i]._rectangle.Size.Height;
			return ItemOffset;
		}

		/// <summary>
		/// Calculate every panel position based on how many items are in the menu + description if present + heritage windows if there are any
		/// </summary>
		private float CalculatePanelsPosition(bool hasDescription)
		{
			float Height = CalculateWindowHeight() + 40 + _mainMenu.Position.Y;
			if (hasDescription)
				Height += _descriptionRectangle.Size.Height + 5;
			return CalculateItemHeight() + Height;
		}


		private void DrawCalculations()
		{
			float WindowHeight = CalculateWindowHeight();
			DrawWidth = new SizeF(431 + WidthOffset, 100);
			Safe = ScreenTools.SafezoneBounds;
			BackgroundSize = Size > MaxItemsOnScreen + 1 ? new SizeF(431 + WidthOffset, 38 * (MaxItemsOnScreen + 1 + WindowHeight)) : new SizeF(431 + WidthOffset, 38 * Size + WindowHeight);
			_extraRectangleUp.Size = new SizeF(431 + WidthOffset, 18 + WindowHeight);
			_extraRectangleDown.Size = new SizeF(431 + WidthOffset, 18 + WindowHeight);
			_upAndDownSprite.Position = new PointF(190 + Offset.X + (WidthOffset > 0 ? (WidthOffset / 2) : WidthOffset), 147 + 37 * (MaxItemsOnScreen + 1) + Offset.Y - 37 + _extraYOffset + WindowHeight);
			ReDraw = false;
			if (MenuItems.Count != 0 && !String.IsNullOrWhiteSpace(MenuItems[_activeItem % (MenuItems.Count)].Description))
			{
				RecalculateDescriptionPosition();
				string descCaption = MenuItems[_activeItem % (MenuItems.Count)].Description;
				_descriptionText.Caption = descCaption;
				_descriptionText.Wrap = 400;
				int numLines = ScreenTools.GetLineCount(descCaption, _descriptionText.Position, _descriptionText.Font, _descriptionText.Scale, _descriptionText.Position.X + 400);
				_descriptionRectangle.Size = new SizeF(431 + WidthOffset, (numLines * 25) + 15);
			}
		}

		/// <summary>
		/// Go up the menu if the number of items is more than maximum items on screen.
		/// </summary>
		public void GoUpOverflow()
		{
			if (Size <= MaxItemsOnScreen + 1) return;
			if (_activeItem % MenuItems.Count <= _minItem)
			{
				if (_activeItem % MenuItems.Count == 0)
				{
					_minItem = MenuItems.Count - MaxItemsOnScreen - 1;
					_maxItem = MenuItems.Count - 1;
					MenuItems[_activeItem % (MenuItems.Count)].Selected = false;
					_activeItem = 1000 - (1000 % MenuItems.Count);
					_activeItem += MenuItems.Count - 1;
					MenuItems[_activeItem % (MenuItems.Count)].Selected = true;
				}
				else
				{
					_minItem--;
					_maxItem--;
					MenuItems[_activeItem % (MenuItems.Count)].Selected = false;
					_activeItem--;
					MenuItems[_activeItem % (MenuItems.Count)].Selected = true;
				}
			}
			else
			{
				MenuItems[_activeItem % (MenuItems.Count)].Selected = false;
				_activeItem--;
				MenuItems[_activeItem % (MenuItems.Count)].Selected = true;
			}
			Game.PlaySound(AUDIO_UPDOWN, AUDIO_LIBRARY);
			IndexChange(CurrentSelection);
		}


		/// <summary>
		/// Go up the menu if the number of items is less than or equal to the maximum items on screen.
		/// </summary>
		public void GoUp()
		{
			if (Size > MaxItemsOnScreen + 1) return;
			MenuItems[_activeItem % (MenuItems.Count)].Selected = false;
			_activeItem--;
			MenuItems[_activeItem % (MenuItems.Count)].Selected = true;
			Game.PlaySound(AUDIO_UPDOWN, AUDIO_LIBRARY);
			IndexChange(CurrentSelection);
		}


		/// <summary>
		/// Go down the menu if the number of items is more than maximum items on screen.
		/// </summary>
		public void GoDownOverflow()
		{
			if (Size <= MaxItemsOnScreen + 1) return;
			if (_activeItem % MenuItems.Count >= _maxItem)
			{
				if (_activeItem % MenuItems.Count == MenuItems.Count - 1)
				{
					_minItem = 0;
					_maxItem = MaxItemsOnScreen;
					MenuItems[_activeItem % (MenuItems.Count)].Selected = false;
					_activeItem = 1000 - (1000 % MenuItems.Count);
					MenuItems[_activeItem % (MenuItems.Count)].Selected = true;
				}
				else
				{
					_minItem++;
					_maxItem++;
					MenuItems[_activeItem % (MenuItems.Count)].Selected = false;
					_activeItem++;
					MenuItems[_activeItem % (MenuItems.Count)].Selected = true;
				}
			}
			else
			{
				MenuItems[_activeItem % (MenuItems.Count)].Selected = false;
				_activeItem++;
				MenuItems[_activeItem % (MenuItems.Count)].Selected = true;
			}
			Game.PlaySound(AUDIO_UPDOWN, AUDIO_LIBRARY);
			IndexChange(CurrentSelection);
		}


		/// <summary>
		/// Go up the menu if the number of items is less than or equal to the maximum items on screen.
		/// </summary>
		public void GoDown()
		{
			if (Size > MaxItemsOnScreen + 1) return;
			MenuItems[_activeItem % (MenuItems.Count)].Selected = false;
			_activeItem++;
			MenuItems[_activeItem % (MenuItems.Count)].Selected = true;
			Game.PlaySound(AUDIO_UPDOWN, AUDIO_LIBRARY);
			IndexChange(CurrentSelection);
		}


		/// <summary>
		/// Go left on a UIMenuListItem & UIMenuSliderItem.
		/// </summary>
		public void GoLeft()
		{
			if (!MenuItems[CurrentSelection].Enabled)
			{
				return;
			}

			if (!(MenuItems[CurrentSelection] is UIMenuListItem) && !(MenuItems[CurrentSelection] is UIMenuSliderItem) && !(MenuItems[CurrentSelection] is UIMenuDynamicListItem) && !(MenuItems[CurrentSelection] is UIMenuSliderProgressItem) && !(MenuItems[CurrentSelection] is UIMenuProgressItem)) return;


			if (MenuItems[CurrentSelection] is UIMenuListItem)
			{
				UIMenuListItem it = (UIMenuListItem)MenuItems[CurrentSelection];
				it.Index -= 1;
				Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
				ListChange(it, it.Index);
				it.ListChangedTrigger(it.Index);
			}
			else if (MenuItems[CurrentSelection] is UIMenuDynamicListItem)
			{
				UIMenuDynamicListItem it = (UIMenuDynamicListItem)MenuItems[CurrentSelection];
				string newItem = it.Callback(it, UIMenuDynamicListItem.ChangeDirection.Left);
				it.CurrentListItem = newItem;
				Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
			}
			else if (MenuItems[CurrentSelection] is UIMenuSliderItem)
			{
				UIMenuSliderItem it = (UIMenuSliderItem)MenuItems[CurrentSelection];
				it.Value -= it.Multiplier;
				Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
				SliderChange(it, it.Value);
			}
			else if (MenuItems[CurrentSelection] is UIMenuSliderProgressItem)
			{
				UIMenuSliderProgressItem it = (UIMenuSliderProgressItem)MenuItems[CurrentSelection];
				it.Value--;
				Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
				SliderProgressChange(it, it.Value);
			}
			else if (MenuItems[CurrentSelection] is UIMenuProgressItem)
			{
				UIMenuProgressItem it = (UIMenuProgressItem)MenuItems[CurrentSelection];
				it.Index--;
				Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
				ProgressChange(it, it.Index);
			}
		}



		/// <summary>
		/// Go right on a UIMenuListItem & UIMenuSliderItem.
		/// </summary>
		public void GoRight()
		{
			if (!MenuItems[CurrentSelection].Enabled)
			{
				return;
			}

			if (!(MenuItems[CurrentSelection] is UIMenuListItem) && !(MenuItems[CurrentSelection] is UIMenuSliderItem) && !(MenuItems[CurrentSelection] is UIMenuDynamicListItem) && !(MenuItems[CurrentSelection] is UIMenuSliderProgressItem) && !(MenuItems[CurrentSelection] is UIMenuProgressItem)) return;

			if (MenuItems[CurrentSelection] is UIMenuListItem)
			{
				UIMenuListItem it = (UIMenuListItem)MenuItems[CurrentSelection];
				it.Index++;
				Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
				ListChange(it, it.Index);
				it.ListChangedTrigger(it.Index);
			}

			else if (MenuItems[CurrentSelection] is UIMenuDynamicListItem)
			{
				UIMenuDynamicListItem it = (UIMenuDynamicListItem)MenuItems[CurrentSelection];
				string newItem = it.Callback(it, UIMenuDynamicListItem.ChangeDirection.Right);
				it.CurrentListItem = newItem;
				Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
			}
			else if (MenuItems[CurrentSelection] is UIMenuSliderItem)
			{
				UIMenuSliderItem it = (UIMenuSliderItem)MenuItems[CurrentSelection];
				it.Value += it.Multiplier;
				Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
				SliderChange(it, it.Value);
			}
			else if (MenuItems[CurrentSelection] is UIMenuSliderProgressItem)
			{
				UIMenuSliderProgressItem it = (UIMenuSliderProgressItem)MenuItems[CurrentSelection];
				it.Value ++;
				Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
				SliderProgressChange(it, it.Value);
			}
			else if (MenuItems[CurrentSelection] is UIMenuProgressItem)
			{
				UIMenuProgressItem it = (UIMenuProgressItem)MenuItems[CurrentSelection];
				it.Index++;
				Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
				ProgressChange(it, it.Index);
			}
		}


		/// <summary>
		/// Activate the current selected item.
		/// </summary>
		public void SelectItem()
		{
			if (!MenuItems[CurrentSelection].Enabled)
			{
				Game.PlaySound(AUDIO_ERROR, AUDIO_LIBRARY);
				return;
			}
			if (MenuItems[CurrentSelection] is UIMenuCheckboxItem)
			{
				UIMenuCheckboxItem it = (UIMenuCheckboxItem)MenuItems[CurrentSelection];
				it.Checked = !it.Checked;
				Game.PlaySound(AUDIO_SELECT, AUDIO_LIBRARY);
				CheckboxChange(it, it.Checked);
				it.CheckboxEventTrigger();
			}
			else if (MenuItems[CurrentSelection] is UIMenuListItem)
			{
				UIMenuListItem it = (UIMenuListItem)MenuItems[CurrentSelection];
				Game.PlaySound(AUDIO_SELECT, AUDIO_LIBRARY);
				ListSelect(it, it.Index);
				it.ListSelectedTrigger(it.Index);
			}
			else
			{
				Game.PlaySound(AUDIO_SELECT, AUDIO_LIBRARY);
				ItemSelect(MenuItems[CurrentSelection], CurrentSelection);
				MenuItems[CurrentSelection].ItemActivate(this);
				if (!Children.ContainsKey(MenuItems[CurrentSelection])) return;
				Children[MenuItems[CurrentSelection]].Visible = true;
				Children[MenuItems[CurrentSelection]].MouseEdgeEnabled = MouseEdgeEnabled;
				Visible = false;
				InstructionalButtonsHandler.InstructionalButtons.Enabled = true;
				InstructionalButtonsHandler.InstructionalButtons.SetInstructionalButtons(Children[MenuItems[CurrentSelection]].InstructionalButtons);
				_poolcontainer.MenuChangeEv(this, Children[MenuItems[CurrentSelection]], MenuState.ChangeForward);
				MenuChangeEv(this, Children[MenuItems[CurrentSelection]], MenuState.ChangeForward);
				Children[MenuItems[CurrentSelection]].MenuChangeEv(this, Children[MenuItems[CurrentSelection]], MenuState.ChangeForward);
			}
		}


		/// <summary>
		/// Close or go back in a menu chain.
		/// </summary>
		public void GoBack()
		{
			Game.PlaySound(AUDIO_BACK, AUDIO_LIBRARY);
			if (ParentMenu != null)
			{
				PointF tmp = new PointF(0.5f, 0.5f);
				ParentMenu.Visible = true;
				InstructionalButtonsHandler.InstructionalButtons.Enabled = true;
				InstructionalButtonsHandler.InstructionalButtons.SetInstructionalButtons(ParentMenu.InstructionalButtons);
				_poolcontainer.MenuChangeEv(this, ParentMenu, MenuState.ChangeBackward);
				ParentMenu.MenuChangeEv(this, ParentMenu, MenuState.ChangeBackward);
				MenuChangeEv(this, ParentMenu, MenuState.ChangeBackward);
				if (ResetCursorOnOpen)
					API.SetCursorLocation(tmp.X, tmp.Y);
				//Cursor.Position = tmp;
			}
			Visible = false;
		}


		/// <summary>
		/// Makes the specified item open a menu when is activated.
		/// </summary>
		/// <param name="menuToBind">The menu that is going to be opened when the item is activated.</param>
		/// <param name="itemToBindTo">The item that is going to activate the menu.</param>
		public void BindMenuToItem(UIMenu menuToBind, UIMenuItem itemToBindTo)
		{
			if (!MenuItems.Contains(itemToBindTo))
				AddItem(itemToBindTo);
			menuToBind.ParentMenu = this;
			menuToBind.ParentItem = itemToBindTo;
			if (Children.ContainsKey(itemToBindTo))
				Children[itemToBindTo] = menuToBind;
			else
				Children.Add(itemToBindTo, menuToBind);
		}


		/// <summary>
		/// Remove menu binding from button.
		/// </summary>
		/// <param name="releaseFrom">Button to release from.</param>
		/// <returns>Returns true if the operation was successful.</returns>
		public bool ReleaseMenuFromItem(UIMenuItem releaseFrom)
		{
			if (!Children.ContainsKey(releaseFrom)) return false;
			Children[releaseFrom].ParentItem = null;
			Children[releaseFrom].ParentMenu = null;
			Children.Remove(releaseFrom);
			return true;
		}

		/// <summary>
		/// Set a key to control a menu. Can be multiple keys for each control.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="keyToSet"></param>
		public void SetKey(MenuControls control, Keys keyToSet)
		{
			if (_keyDictionary.ContainsKey(control))
				_keyDictionary[control].Item1.Add(keyToSet);
			else
			{
				_keyDictionary.Add(control,
					new Tuple<List<Keys>, List<Tuple<Control, int>>>(new List<Keys>(), new List<Tuple<Control, int>>()));
				_keyDictionary[control].Item1.Add(keyToSet);
			}
		}


		/// <summary>
		/// Set a GTA.Control to control a menu. Can be multiple controls. This applies it to all indexes.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="gtaControl"></param>
		public void SetKey(MenuControls control, Control gtaControl)
		{
			SetKey(control, gtaControl, 0);
			SetKey(control, gtaControl, 1);
			SetKey(control, gtaControl, 2);
		}


		/// <summary>
		/// Set a GTA.Control to control a menu only on a specific index.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="gtaControl"></param>
		/// <param name="controlIndex"></param>
		public void SetKey(MenuControls control, Control gtaControl, int controlIndex)
		{
			if (_keyDictionary.ContainsKey(control))
				_keyDictionary[control].Item2.Add(new Tuple<Control, int>(gtaControl, controlIndex));
			else
			{
				_keyDictionary.Add(control,
					new Tuple<List<Keys>, List<Tuple<Control, int>>>(new List<Keys>(), new List<Tuple<Control, int>>()));
				_keyDictionary[control].Item2.Add(new Tuple<Control, int>(gtaControl, controlIndex));
			}

		}


		/// <summary>
		/// Remove all controls on a control.
		/// </summary>
		/// <param name="control"></param>
		public void ResetKey(MenuControls control)
		{
			_keyDictionary[control].Item1.Clear();
			_keyDictionary[control].Item2.Clear();
		}


		/// <summary>
		/// Check whether a menucontrol has been pressed.
		/// </summary>
		/// <param name="control">Control to check for.</param>
		/// <param name="key">Key if you're using keys.</param>
		/// <returns></returns>
		public bool HasControlJustBeenPressed(MenuControls control, Keys key = Keys.None)
		{
			List<Keys> tmpKeys = new List<Keys>(_keyDictionary[control].Item1);
			List<Tuple<Control, int>> tmpControls = new List<Tuple<Control, int>>(_keyDictionary[control].Item2);

			if (key != Keys.None)
			{
				//if (tmpKeys.Any(Game.IsKeyPressed))
				//    return true;
			}
			if (tmpControls.Any(tuple => Game.IsControlJustPressed(tuple.Item2, tuple.Item1)))
				return true;
			return false;
		}


		/// <summary>
		/// Check whether a menucontrol has been released.
		/// </summary>
		/// <param name="control">Control to check for.</param>
		/// <param name="key">Key if you're using keys.</param>
		/// <returns></returns>
		public bool HasControlJustBeenReleaseed(MenuControls control, Keys key = Keys.None)
		{
			List<Keys> tmpKeys = new List<Keys>(_keyDictionary[control].Item1);
			List<Tuple<Control, int>> tmpControls = new List<Tuple<Control, int>>(_keyDictionary[control].Item2);

			if (key != Keys.None)
			{
				//if (tmpKeys.Any(Game.IsKeyPressed))
				//    return true;
			}
			if (tmpControls.Any(tuple => Game.IsControlJustReleased(tuple.Item2, tuple.Item1)))
				return true;
			return false;
		}

		private int _controlCounter;
		/// <summary>
		/// Check whether a menucontrol is being pressed.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool IsControlBeingPressed(MenuControls control, Keys key = Keys.None)
		{
			List<Keys> tmpKeys = new List<Keys>(_keyDictionary[control].Item1);
			List<Tuple<Control, int>> tmpControls = new List<Tuple<Control, int>>(_keyDictionary[control].Item2);
			if (HasControlJustBeenReleaseed(control, key)) _controlCounter = 0;
			if (_controlCounter > 0)
			{
				_controlCounter++;
				if (_controlCounter > 30)
					_controlCounter = 0;
				return false;
			}
			if (key != Keys.None)
			{
				//if (tmpKeys.Any(Game.IsKeyPressed))
				//{
				//    _controlCounter = 1;
				//    return true;
				//}
			}
			if (tmpControls.Any(tuple => Game.IsControlPressed(tuple.Item2, tuple.Item1)))
			{
				_controlCounter = 1;
				return true;
			}
			return false;
		}

		[Obsolete("Use InstructionalButtons.Add instead")]
		public void AddInstructionalButton(InstructionalButton button)
		{
			//_instructionalButtons.Add(button);
		}

		[Obsolete("Use InstructionalButtons.Remove instead")]
		public void RemoveInstructionalButton(InstructionalButton button)
		{
			//_instructionalButtons.Remove(button);
		}

		#endregion

		#region Private Methods
		private void RecalculateDescriptionPosition()
		{
			//_descriptionText.WordWrap = new SizeF(425 + WidthOffset, 0);

			var WindowHeight = CalculateWindowHeight();

			_descriptionBar.Position = new PointF(Offset.X, 149 - 37 + _extraYOffset + Offset.Y + WindowHeight);
			_descriptionRectangle.Position = new PointF(Offset.X, 149 - 37 + _extraYOffset + Offset.Y + WindowHeight);
			_descriptionText.Position = new PointF(Offset.X + 8, 155 - 37 + _extraYOffset + Offset.Y + WindowHeight);

			_descriptionBar.Size = new SizeF(431 + WidthOffset, 4);
			_descriptionRectangle.Size = new SizeF(431 + WidthOffset, 30);

			int count = Size;
			if (count > MaxItemsOnScreen + 1)
				count = MaxItemsOnScreen + 2;

			_descriptionBar.Position = new PointF(Offset.X, 38 * count + _descriptionBar.Position.Y);
			_descriptionRectangle.Position = new PointF(Offset.X, 38 * count + _descriptionRectangle.Position.Y);
			_descriptionText.Position = new PointF(Offset.X + 8, 38 * count + _descriptionText.Position.Y);
		}

		/// <summary>
		/// Function to get whether the cursor is in an arrow space, or in label of an UIMenuListItem.
		/// </summary>
		/// <param name="item">What item to check</param>
		/// <param name="topLeft">top left point of the item.</param>
		/// <param name="safezone">safezone size.</param>
		/// <returns>0 - Not in item at all, 1 - In label, 2 - In arrow space.</returns>
		private int IsMouseInListItemArrows(UIMenuItem item, PointF topLeft, PointF safezone) // TODO: Ability to scroll left and right
		{
			API.BeginTextCommandWidth("jamyfafi");
			UIResText.AddLongString(item.Text);
			float screenw = Resolution.Width;
			float screenh = Resolution.Height;
			const float height = 1080f;
			float ratio = screenw / screenh;
			float width = height * ratio;
			int labelSize = (int)(API.EndTextCommandGetWidth(false) * width * 0.35f);

			int labelSizeX = 5 + labelSize + 10;
			int arrowSizeX = 431 - labelSizeX;
			return ScreenTools.IsMouseInBounds(topLeft, new SizeF(labelSizeX, 38))
				? 1
				: ScreenTools.IsMouseInBounds(new PointF(topLeft.X + labelSizeX, topLeft.Y), new SizeF(arrowSizeX, 38)) ? 2 : 0;

		}

		#endregion

		#region Drawing & Processing
		/// <summary>
		/// Draw the menu and all of it's components.
		/// </summary>
		public async Task Draw()
		{
			if (!Visible || PopupWarningThread.Warning.IsShowing) return;
			if (ControlDisablingEnabled)
				Controls.Toggle(false);

			if (ScaleWithSafezone)
			{
				API.SetScriptGfxAlign(76, 84); // Safezone
				API.SetScriptGfxAlignParams(0f, 0f, 0f, 0f); // stuff
			}

			if (ReDraw) DrawCalculations();

			if (String.IsNullOrWhiteSpace(BannerTexture))
			{
				if (BannerSprite  != null)
					BannerSprite.Draw();
				else
				{
					BannerRectangle?.Draw();
				}
			}
			else
			{
				PointF start = ((ScaleWithSafezone) ? Safe : new PointF(0, 0));

				//Sprite.DrawTexture(BannerTexture, new PointF(start.X + Offset.X, start.Y + Offset.Y), DrawWidth);
			}
			_mainMenu.Draw();
			if (Glare)
			{
				_menuGlare.CallFunction("SET_DATA_SLOT", GameplayCamera.RelativeHeading);
				SizeF _glareSize = new SizeF(1.0f, 1.054f);
				PointF gl = new PointF(
					(Offset.X / Resolution.Width) + 0.4491f,
					(Offset.Y / Resolution.Height) + 0.475f
				);

				API.DrawScaleformMovie(_menuGlare.Handle, gl.X, gl.Y, _glareSize.Width, _glareSize.Height, 255, 255, 255, 255, 0);
			}
			if (MenuItems.Count == 0 && Windows.Count == 0)
			{
				API.ResetScriptGfxAlign(); // Safezone end
				return;
			}

			_background.Size = BackgroundSize;

			_background.Draw();

			MenuItems[_activeItem % (MenuItems.Count)].Selected = true;
			if (!String.IsNullOrWhiteSpace(MenuItems[_activeItem % (MenuItems.Count)].Description))
			{
				_descriptionBar.Draw();
				_descriptionRectangle.Draw();
				_descriptionText.Draw();
			}

			var WindowHeight = CalculateWindowHeight();

			if (MenuItems.Count <= MaxItemsOnScreen + 1)
			{
				int count = 0;

				foreach (UIMenuItem item in MenuItems)
				{
					item.Position(count * 38 - 37 + _extraYOffset + (int)Math.Round(WindowHeight));
					item.Draw();
					count++;
				}
			}
			else
			{
				int count = 0;

				for (int index = _minItem; index <= _maxItem; index++)
				{
					UIMenuItem item = MenuItems[index];
					item.Position(count * 38 - 37 + _extraYOffset + (int)Math.Round(WindowHeight));
					item.Draw();
					count++;
				}

				_extraRectangleUp.Draw();
				_extraRectangleDown.Draw();
				_upAndDownSprite.Draw();

				if (_counterText != null)
				{
					string cap = (CurrentSelection + 1) + " / " + Size;
					_counterText.Caption = CounterPretext + cap;
					_counterText.Draw();
				}
			}

			if (Windows.Count > 0)
			{
				float WindowOffset = 0;
				for (int index = 0; index < Windows.Count; index++)
				{
					if (index > 0)
						WindowOffset += Windows[index].Background.Size.Height;
					Windows[index].Position(WindowOffset + _extraYOffset + 37);
					Windows[index].Draw();
				}
			}

			if (MenuItems[CurrentSelection] is UIMenuListItem)
			{
				if ((MenuItems[CurrentSelection] as UIMenuListItem).Panels.Count > 0)
				{
					PanelOffset = CalculatePanelsPosition(!String.IsNullOrWhiteSpace(MenuItems[CurrentSelection].Description));
					for (int i = 0; i < (MenuItems[CurrentSelection] as UIMenuListItem).Panels.Count; i++)
					{
						if (i > 0)
							PanelOffset = PanelOffset + (MenuItems[CurrentSelection] as UIMenuListItem).Panels[i - 1].Background.Size.Height + 5;
						(MenuItems[CurrentSelection] as UIMenuListItem).Panels[i].Position(PanelOffset);
						(MenuItems[CurrentSelection] as UIMenuListItem).Panels[i].Draw();
					}
				}
			}
			if (ScaleWithSafezone)
				API.ResetScriptGfxAlign(); // Safezone end
		}

		/// <summary>
		/// Process the mouse's position and check if it's hovering over any UI element. Call this in OnTick
		/// </summary>
		public void ProcessMouse()
		{
			var WindowHeight = CalculateWindowHeight();
			if (!Visible || _justOpened || MenuItems.Count == 0 || IsUsingController || !MouseControlsEnabled)
			{
				API.EnableControlAction(2, (int)Control.LookUpDown, true);
				API.EnableControlAction(2, (int)Control.LookLeftRight, true);
				API.EnableControlAction(2, (int)Control.Aim, true);
				API.EnableControlAction(2, (int)Control.Attack, true);
				if (_itemsDirty)
				{
					MenuItems.Where(i => i.Hovered).ToList().ForEach(i => i.Hovered = false);
					_itemsDirty = false;
				}
				return;
			}

			PointF safezoneOffset = ScreenTools.SafezoneBounds;
			API.ShowCursorThisFrame();
			int limit = MenuItems.Count - 1;
			int counter = 0;
			if (MenuItems.Count > MaxItemsOnScreen + 1)
				limit = _maxItem;

			if (ScreenTools.IsMouseInBounds(new PointF(0, 0), new SizeF(30, 1080)) && MouseEdgeEnabled)
			{
				GameplayCamera.RelativeHeading += 5f;
				API.SetCursorSprite(6);
			}
			else if (ScreenTools.IsMouseInBounds(new PointF(Convert.ToInt32(Resolution.Width - 30f), 0), new SizeF(30, 1080)) && MouseEdgeEnabled)
			{
				GameplayCamera.RelativeHeading -= 5f;
				API.SetCursorSprite(7);
			}
			else if (MouseEdgeEnabled)
			{
				API.SetCursorSprite(1);
			}

			for (int i = _minItem; i <= limit; i++)
			{
				float xpos = Offset.X + safezoneOffset.X;
				float ypos = Offset.Y + 144 - 37 + _extraYOffset + (counter * 38) + safezoneOffset.Y + WindowHeight;
				float yposSelected = Offset.Y + 144 - 37 + _extraYOffset + safezoneOffset.Y + CurrentSelection * 38 + WindowHeight;
				int xsize = 431 + WidthOffset;
				const int ysize = 38;
				UIMenuItem uiMenuItem = MenuItems[i];
				if (ScreenTools.IsMouseInBounds(new PointF(xpos, ypos), new SizeF(xsize, ysize)))
				{
					uiMenuItem.Hovered = true;
					int res = IsMouseInListItemArrows(MenuItems[i], new PointF(xpos, yposSelected),
						safezoneOffset);
					if (uiMenuItem.Hovered && res == 1 && MenuItems[i] is IListItem)
					{
						API.SetMouseCursorSprite(5);
					}
					if (Game.IsControlJustPressed(0, Control.Attack))
						if (uiMenuItem.Selected && uiMenuItem.Enabled)
						{
							if (MenuItems[i] is IListItem &&
								IsMouseInListItemArrows(MenuItems[i], new PointF(xpos, ypos),
									safezoneOffset) > 0)
							{
								switch (res)
								{
									case 1:
										SelectItem();
										break;
									case 2:
										GoRight();
										break;
								}
							}
							else
								SelectItem();
						}
						else if (!uiMenuItem.Selected)
						{
							CurrentSelection = i;
							Game.PlaySound(AUDIO_UPDOWN, AUDIO_LIBRARY);
							IndexChange(CurrentSelection);
						}
						else if (!uiMenuItem.Enabled && uiMenuItem.Selected)
						{
							Game.PlaySound(AUDIO_ERROR, AUDIO_LIBRARY);
						}
				}
				else
					uiMenuItem.Hovered = false;
				counter++;
			}
			float extraY = 144 + 38 * (MaxItemsOnScreen + 1) + Offset.Y - 37 + _extraYOffset + safezoneOffset.Y + WindowHeight;
			float extraX = safezoneOffset.X + Offset.X;
			if (Size <= MaxItemsOnScreen + 1) return;
			if (ScreenTools.IsMouseInBounds(new PointF(extraX, extraY), new SizeF(431 + WidthOffset, 18)))
			{
				_extraRectangleUp.Color = Color.FromArgb(255, 30, 30, 30);
				if (Game.IsControlJustPressed(0, Control.Attack))
				{
					if (Size > MaxItemsOnScreen + 1)
						GoUpOverflow();
					else
						GoUp();
				}
			}
			else
				_extraRectangleUp.Color = Color.FromArgb(200, 0, 0, 0);

			if (ScreenTools.IsMouseInBounds(new PointF(extraX, extraY + 18), new SizeF(431 + WidthOffset, 18)))
			{
				_extraRectangleDown.Color = Color.FromArgb(255, 30, 30, 30);
				if (Game.IsControlJustPressed(0, Control.Attack))
				{
					if (Size > MaxItemsOnScreen + 1)
						GoDownOverflow();
					else
						GoDown();
				}
			}
			else
				_extraRectangleDown.Color = Color.FromArgb(200, 0, 0, 0);
		}

		/// <summary>
		/// Process control-stroke. Call this in the OnTick event.
		/// </summary>
		public void ProcessControl(Keys key = Keys.None)
		{
			if (!Visible || PopupWarningThread.Warning.IsShowing) return;
			if (_justOpened)
			{
				_justOpened = false;
				return;
			}

			if (HasControlJustBeenReleaseed(MenuControls.Back, key) && API.UpdateOnscreenKeyboard() != 0 && !API.IsWarningMessageActive())
			{
				GoBack();
			}
			if (MenuItems.Count == 0) return;
			if (IsControlBeingPressed(MenuControls.Up, key) && API.UpdateOnscreenKeyboard() != 0 && !API.IsWarningMessageActive())
			{
				if (API.GetGameTimer() - _pressingTimer > 175)
				{
					if (Size > MaxItemsOnScreen + 1)
						GoUpOverflow();
					else
						GoUp();
					_pressingTimer = API.GetGameTimer();
				}
			}

			else if (IsControlBeingPressed(MenuControls.Down, key) && API.UpdateOnscreenKeyboard() != 0 && !API.IsWarningMessageActive())
			{
				if (API.GetGameTimer() - _pressingTimer > 175)
				{
					if (Size > MaxItemsOnScreen + 1)
						GoDownOverflow();
					else
						GoDown();
					_pressingTimer = API.GetGameTimer();
				}
			}

			else if (IsControlBeingPressed(MenuControls.Left, key) && API.UpdateOnscreenKeyboard() != 0 && !API.IsWarningMessageActive())
			{
				if (API.GetGameTimer() - _pressingTimer > 175)
				{
					GoLeft();
					_pressingTimer = API.GetGameTimer();
				}
			}

			else if (IsControlBeingPressed(MenuControls.Right, key) && API.UpdateOnscreenKeyboard() != 0 && !API.IsWarningMessageActive())
			{
				if (API.GetGameTimer() - _pressingTimer > 175)
				{
					GoRight();
					_pressingTimer = API.GetGameTimer();
				}
			}

			else if (HasControlJustBeenPressed(MenuControls.Select, key) && API.UpdateOnscreenKeyboard() != 0 && !API.IsWarningMessageActive())
			{
				SelectItem();
			}

		}

		/// <summary>
		/// Process keystroke. Call this in the OnKeyDown event.
		/// </summary>
		/// <param name="key"></param>
		public void ProcessKey(Keys key)
		{
			if ((from MenuControls menuControl in _menuControls
				 select new List<Keys>(_keyDictionary[menuControl].Item1))
				.Any(tmpKeys => tmpKeys.Any(k => k == key)))
			{
				ProcessControl(key);
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Change whether this menu is visible to the user.
		/// </summary>
		public bool Visible
		{
			get { return _visible; }
			set
			{
				_visible = value;
				_justOpened = value;
				_itemsDirty = value;

				if (ParentMenu != null) return;
				if (Children.ContainsKey(MenuItems[CurrentSelection]) && Children[MenuItems[CurrentSelection]].Visible) return;
				InstructionalButtonsHandler.InstructionalButtons.Enabled = value;
				InstructionalButtonsHandler.InstructionalButtons.SetInstructionalButtons(InstructionalButtons);
				if (value)
				{
					_poolcontainer.MenuChangeEv(null, this, MenuState.Opened);
					MenuChangeEv(null, this, MenuState.Opened);
				}
				else
				{
					_poolcontainer.MenuChangeEv(this, null, MenuState.Closed);
					MenuChangeEv(this, null, MenuState.Closed);
				}
				if (!value) return;
				if (!ResetCursorOnOpen) return;
				API.SetCursorLocation(0.5f, 0.5f);
				Screen.Hud.CursorSprite = CursorSprite.Normal;
			}
		}


		/// <summary>
		/// Returns the current selected item's index.
		/// Change the current selected item to index. Use this after you add or remove items dynamically.
		/// </summary>
		public int CurrentSelection
		{
			get { return MenuItems.Count == 0 ? 0 : _activeItem % MenuItems.Count; }
			set
			{
				if (MenuItems.Count == 0) _activeItem = 0;
				MenuItems[_activeItem % (MenuItems.Count)].Selected = false;
				_activeItem = 1000000 - (1000000 % MenuItems.Count) + value;
				if (CurrentSelection > _maxItem)
				{
					_maxItem = CurrentSelection;
					_minItem = CurrentSelection - MaxItemsOnScreen;
				}
				else if (CurrentSelection < _minItem)
				{
					_maxItem = MaxItemsOnScreen + CurrentSelection;
					_minItem = CurrentSelection;
				}
			}
		}

		/// <summary>
		/// Returns false if last input was made with mouse and keyboard, true if it was made with a controller.
		/// </summary>
		public static bool IsUsingController => !API.IsInputDisabled(2);


		/// <summary>
		/// Returns the amount of items in the menu.
		/// </summary>
		public int Size => MenuItems.Count;


		/// <summary>
		/// Returns the title object.
		/// </summary>
		public UIResText Title { get; }


		/// <summary>
		/// Returns the subtitle object.
		/// </summary>
		public UIResText Subtitle { get; }


		/// <summary>
		/// String to pre-attach to the counter string. Useful for color codes.
		/// </summary>
		public string CounterPretext { get; set; }


		/// <summary>
		/// If this is a nested menu, returns the parent menu. You can also set it to a menu so when pressing Back it goes to that menu.
		/// </summary>
		public UIMenu ParentMenu { get; set; }


		/// <summary>
		/// If this is a nested menu, returns the item it was bound to.
		/// </summary>
		public UIMenuItem ParentItem { get; set; }

		//Tree structure
		public Dictionary<UIMenuItem, UIMenu> Children { get; }

		/// <summary>
		/// Returns the current width offset.
		/// </summary>
		public int WidthOffset { get; private set; }

		#endregion

		#region Event Invokers
		protected virtual void IndexChange(int newindex)
		{
			ReDraw = true;

			OnIndexChange?.Invoke(this, newindex);
		}

		internal virtual void ListChange(UIMenuListItem sender, int newindex)
		{
			OnListChange?.Invoke(this, sender, newindex);
		}

		internal virtual void ProgressChange(UIMenuProgressItem sender, int newindex)
		{
			OnProgressChange?.Invoke(this, sender, newindex);
		}

		protected virtual void ListSelect(UIMenuListItem sender, int newindex)
		{
			OnListSelect?.Invoke(this, sender, newindex);
		}

		protected virtual void SliderChange(UIMenuSliderItem sender, int newindex)
		{
			OnSliderChange?.Invoke(this, sender, newindex);
		}

		internal virtual void SliderProgressChange(UIMenuSliderProgressItem sender, int newindex)
		{
			OnProgressSliderChange?.Invoke(this, sender, newindex);
		}

		protected virtual void ItemSelect(UIMenuItem selecteditem, int index)
		{
			OnItemSelect?.Invoke(this, selecteditem, index);
		}

		protected virtual void CheckboxChange(UIMenuCheckboxItem sender, bool Checked)
		{
			OnCheckboxChange?.Invoke(this, sender, Checked);
		}

		protected virtual void MenuChangeEv(UIMenu oldmenu, UIMenu newmenu, MenuState state)
		{
			OnMenuStateChanged?.Invoke(oldmenu, newmenu, state);
		}

		#endregion

		public enum MenuControls
		{
			Up,
			Down,
			Left,
			Right,
			Select,
			Back
		}

	}
}

