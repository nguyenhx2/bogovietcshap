﻿using System.Windows.Forms;
using WindowsInput;
using System.Collections.Generic;
using System.Text;
using WindowsInput.Native;
using System.Linq;

namespace BoGoViet.TiengViet
{
    struct MyKey
    {
        public Keys key;
        public bool isUpper;
        public MyKey(Keys k, bool i)
        {
            key = k;
            isUpper = i;
        }
    };

    class TiengViet
    {
        private InputSimulator sim;

        private List<Keys> phuAmDau = new List<Keys>();
        private List<Keys> nguyenAmGiua = new List<Keys>();
        private List<MyKey> phuAmCuoi = new List<MyKey>();

        // neu viet tieng anh
        private List<Keys> nguyenAmGiuaThuHai = new List<Keys>();

        private Keys dauCuoiCau = Keys.None;
        private Keys dauPhuAm = Keys.None;
        private Keys doubleCuoiCau = Keys.None;
        private bool vekepPress = false;
        private bool PrintWW = false;
        private StringBuilder nguyenAmGiuaBienDoi = new StringBuilder();
        private StringBuilder nguyenAmGiuaBienDoiCoDau = new StringBuilder();
        private bool layNguyenAmGiua = true;
        private Keys rememberDauCuoiCau = Keys.None;
        private bool pressLeftShift = false;
        private bool pressRightShift = false;
        private bool[] upperNguyenAm;
        private bool isCapsLockOn = false;

        private bool isCtrlAltWinPress = false;
        private bool isDoubleD = false;
        private string kieuGo; 
        private TiengvietUtil tvu = new TiengvietUtil();

        public TiengViet()
        {
            sim = new InputSimulator();
            upperNguyenAm = new bool[] { false, false, false };
        }

        public void SetKieuGo(string _kieugo)
        {
            this.kieuGo = _kieugo;
            tvu.SetKieuGo(_kieugo);
        }

        public void OnKeyDown(ref object sender, ref KeyEventArgs e)
        {
            
            isCapsLockOn = sim.InputDeviceState.IsTogglingKeyInEffect(VirtualKeyCode.CAPITAL);
            
            if (e.KeyCode == Keys.LShiftKey)
            {
                pressLeftShift = true;
            }
            else if (e.KeyCode == Keys.RShiftKey)
            {
                pressRightShift = true;
            }
            else if (e.KeyCode == Keys.LControlKey
              || e.KeyCode == Keys.RControlKey
              || e.KeyCode == Keys.LWin
              || e.KeyCode == Keys.RWin
              || e.KeyCode == Keys.RMenu
              || e.KeyCode == Keys.LMenu)
            {
                isCtrlAltWinPress = true;
            }
            if (isCtrlAltWinPress)
            {
                Reset();
                return;
            }
            bool toShiftPress = isShiftPress();
            // kiem tra gi
            if (phuAmDau.Count == 1 && phuAmDau.ToArray()[0] == Keys.G &&
                nguyenAmGiua.Count == 1 && nguyenAmGiua.ToArray()[0] == Keys.I
                && tvu.CheckDau(e.KeyCode, isShiftPress()) == -1)
            {
                // PrintWW = false;
                phuAmDau.Add(Keys.I);
                nguyenAmGiua.Clear();
                nguyenAmGiuaThuHai.Clear();
                nguyenAmGiuaBienDoi = new StringBuilder();
                nguyenAmGiuaBienDoiCoDau = new StringBuilder();
                upperNguyenAm[0] = false;
                upperNguyenAm[1] = false;
                upperNguyenAm[2] = false;
            }

            if (tvu.CheckKeyEndWord(e.KeyCode))
            {
                Reset();
                return;
            }
            else if (nguyenAmGiuaThuHai.Count > 0)
            {
                return;
            }
            else if (!IsAZKey(e.KeyCode))
            {
                return;
            }
            // kiem tra la QU
            else if (phuAmDau.Count == 1 && e.KeyCode == Keys.U && phuAmDau.ToArray()[0] == Keys.Q)
            {
                phuAmDau.Add(e.KeyCode);
                layNguyenAmGiua = false;
            }
            else
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.O || e.KeyCode == Keys.U || 
                e.KeyCode == Keys.E || e.KeyCode == Keys.I || e.KeyCode == Keys.Y)
            {
                layNguyenAmGiua = true;
                if (tvu.IsTelexDouble(e.KeyCode)) { 
                    doubleCuoiCau = Keys.None;
                    Keys[] nguyenAmGiuaKeys = nguyenAmGiua.ToArray();
                    bool checkCanDoubleKey = false;
                    for (int i = 0; i < nguyenAmGiua.Count; i++)
                    {
                        if (nguyenAmGiuaKeys[i] == e.KeyCode)
                        {
                            checkCanDoubleKey = true;
                            break;
                        }
                    }
                    if (checkCanDoubleKey)
                    {
                        doubleCuoiCau = e.KeyCode;
                    }
                }
                if (phuAmCuoi.Count == 0 && doubleCuoiCau == Keys.None)
                {
                    nguyenAmGiua.Add(e.KeyCode);

                    // luu nguyen am chu hoa vo mang
                    if (isUpperCase() && nguyenAmGiua.Count < 4)
                    {
                        upperNguyenAm[nguyenAmGiua.Count - 1] = true;
                    }
                }
                else if (doubleCuoiCau == Keys.None)
                {
                    nguyenAmGiuaThuHai.Add(e.KeyCode);
                }
            }
            // VNI 
            else if (tvu.IsVNIDouble(e.KeyCode))
            {
                doubleCuoiCau = Keys.None;
                Keys[] nguyenAmGiuaKeys = nguyenAmGiua.ToArray();
                bool checkCanDoubleKey = false;
                for (int i = 0; i < nguyenAmGiua.Count; i++)
                {
                    if (nguyenAmGiuaKeys[i] == Keys.A || nguyenAmGiuaKeys[i] == Keys.E || nguyenAmGiuaKeys[i] == Keys.O)
                    {
                        checkCanDoubleKey = true;
                        break;
                    }
                }
                if (checkCanDoubleKey)
                {
                    doubleCuoiCau = e.KeyCode;
                }
            }
            else if (phuAmDau.Count == 1 && tvu.IsDGach(e.KeyCode))
            {
                dauPhuAm = e.KeyCode;
                layNguyenAmGiua = true;
            }
            else if (nguyenAmGiua.Count == 0)
            {
                /*
                if (e.KeyCode == Keys.W) {
                    nguyenAmGiua.Add(Keys.U);

                    // luu nguyen am chu hoa vo mang
                    if (isUpperCase() && nguyenAmGiua.Count < 3)
                    {
                        upperNguyenAm[nguyenAmGiua.Count - 1] = true;
                    }
                    PrintWW = true;
                } else
                {
                    phuAmDau.Add(e.KeyCode);
                }
                */
                phuAmDau.Add(e.KeyCode);
                layNguyenAmGiua = false;
            }
            /*
            else if (nguyenAmGiua.Count == 1 && 
                nguyenAmGiuaBienDoi.ToString() == "ư")
            {
                e.Handled = true;
                nguyenAmGiuaBienDoi = new StringBuilder("w");
                sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                SendText("w", 0);
                phuAmDau.Add(Keys.W);
                return;
            }
            */
            else if (tvu.CheckDau(e.KeyCode, isShiftPress()) != -1)
            {
                dauCuoiCau = e.KeyCode;
                layNguyenAmGiua = false;
            }
            else if (tvu.IsTelexDauMoc(e.KeyCode) || 
                tvu.IsVNIDauMocA(e.KeyCode) || tvu.IsVNIDauMocO(e.KeyCode))
            {
                layNguyenAmGiua = false;
                vekepPress = true;
            }
            else
            {
                layNguyenAmGiua = false;
                if (!IsControlKeyCode(e.KeyCode))
                {
                    phuAmCuoi.Add(new MyKey(e.KeyCode, isUpperCase()));
                }
            }

            // prevent send key
            if (doubleCuoiCau != Keys.None || dauCuoiCau != Keys.None || vekepPress == true)
            {
                layNguyenAmGiua = false;
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
            // all thing to write

            Run(ref e);
        }

        private void Run(ref KeyEventArgs e)
        {

            bool printCurrentPressKey = false;
            if (doubleCuoiCau == Keys.None && dauCuoiCau == Keys.None && vekepPress == false
                && PrintWW == false && dauPhuAm == Keys.None)
            {

                if (nguyenAmGiuaBienDoi.Length > 0)
                {
                    if (e.KeyCode == Keys.A || e.KeyCode == Keys.O || e.KeyCode == Keys.U
                        || e.KeyCode == Keys.E || e.KeyCode == Keys.I || e.KeyCode == Keys.Y)
                    {
                        nguyenAmGiuaBienDoi = nguyenAmGiuaBienDoi.Append(e.KeyCode.ToString().ToLower()[0]);
                    }
                }
                else if (nguyenAmGiuaBienDoi.Length == 0 && layNguyenAmGiua)
                {
                    if (e.KeyCode == Keys.A || e.KeyCode == Keys.O || e.KeyCode == Keys.U
                        || e.KeyCode == Keys.E || e.KeyCode == Keys.I || e.KeyCode == Keys.Y)
                    {
                        nguyenAmGiuaBienDoi = ConvertKeysToString(nguyenAmGiua);
                    }
                }
                if (IsAZKey(e.KeyCode))
                {

                }
                return;
            }

            if (tvu.CheckKeyEndWord(e.KeyCode))
            {
                e.Handled = false;
                Reset();
                return;
            }
            else
            {
                e.Handled = true;
            }

            
            //m_Events.KeyPress -= HookManager_KeyPress;

            int numberKeysPhuAmCuoiCau = phuAmCuoi.Count;
            int backIndex = 0;
            if (pressLeftShift)
            {
                sim.Keyboard.KeyUp(VirtualKeyCode.LSHIFT);
            }
            if (pressRightShift)
            {
                sim.Keyboard.KeyUp(VirtualKeyCode.RSHIFT);
            }
            for (int i = 0; i < numberKeysPhuAmCuoiCau; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
            }

            if (doubleCuoiCau != Keys.None)
            {

                // TODO thay doi truong hop co dau
                string textSend = "";

                if (tvu.IsTelexDouble(e.KeyCode) || tvu.IsVNIDouble(e.KeyCode))
                {

                    int checkDau = KiemTraDau(ref textSend, e.KeyCode);
                    if (textSend != "")
                    {
                        for (int i = 0; i < nguyenAmGiuaBienDoi.Length; i++)
                        {
                            sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                        }
                        if (rememberDauCuoiCau != Keys.None)
                        {
                            textSend = ThayDoiDoubleCoDau(textSend, rememberDauCuoiCau);
                            rememberDauCuoiCau = Keys.None;
                        }
                        SendText(textSend);
                    }
                    if (checkDau == 1)
                    {
                        printCurrentPressKey = true;
                        nguyenAmGiuaBienDoi = new StringBuilder();
                    }
                    if (checkDau == 2)
                    {
                        e.Handled = false;
                    }
                }
                doubleCuoiCau = Keys.None;
            }
            /*
            else if (PrintWW == true)
            {
                e.Handled = true;
                nguyenAmGiuaBienDoi = new StringBuilder("ư");
                SendText("ư", 0);
                PrintWW = false;
            }
            */
            else if (vekepPress == true)
            {
                string textSend = "";
                bool check = KiemTraVekep(ref textSend, e.KeyCode);

                if (textSend != "")
                {
                    for (int i = 0; i < nguyenAmGiuaBienDoi.Length; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    }
                    if (rememberDauCuoiCau != Keys.None)
                    {
                        textSend = ThayDoiDoubleCoDau(textSend, rememberDauCuoiCau);
                        rememberDauCuoiCau = Keys.None;
                    }
                    SendText(textSend);
                    vekepPress = false;
                }
                if (textSend != "" && check == false)
                {
                    printCurrentPressKey = true;
                }
                if (textSend == "")
                {
                    e.Handled = false;
                }
                vekepPress = false;
            }
            else if (tvu.IsDGach(e.KeyCode))
            {
                if (phuAmDau.Count == 1 && phuAmDau.ToArray()[0] == Keys.D && isDoubleD == false)
                {
                    for (int i = 0; i < nguyenAmGiuaBienDoi.Length; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                    }
                    sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    SendText("đ", 0);
                    for (int i = 0; i < nguyenAmGiuaBienDoi.Length; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    }
                    isDoubleD = true;
                }
                else if (phuAmDau.Count == 1 && phuAmDau.ToArray()[0] == Keys.D && isDoubleD == true)
                {
                    for (int i = 0; i < nguyenAmGiuaBienDoi.Length; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                    }
                    sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    SendText("d", 0);
                    for (int i = 0; i < nguyenAmGiuaBienDoi.Length; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    }
                    e.Handled = false;
                }
                else
                {
                    e.Handled = false;
                }
                dauPhuAm = Keys.None;
            }
            else if (dauCuoiCau != Keys.None)
            {

                rememberDauCuoiCau = dauCuoiCau;
                string textSend = "";
                bool check = KiemTraDauCuoiCau(ref textSend, dauCuoiCau);
                if (textSend != "")
                {
                    for (int i = 0; i < nguyenAmGiuaBienDoi.Length; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    }
                    SendText(textSend);
                    vekepPress = false;
                    dauCuoiCau = Keys.None;
                }
                if (!check)
                {
                    e.Handled = false;
                }
                dauCuoiCau = Keys.None;
            }

            if (e.Handled == false)
            {
                phuAmCuoi.Add(new MyKey(e.KeyCode, isUpperCase()));
                nguyenAmGiuaThuHai.Add(Keys.NoName);
            }
            for (int i = 0; i < numberKeysPhuAmCuoiCau + backIndex; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
            }
            if (pressLeftShift)
            {
                sim.Keyboard.KeyDown(VirtualKeyCode.LSHIFT);
            }
            if (pressRightShift)
            {
                sim.Keyboard.KeyDown(VirtualKeyCode.RSHIFT);
            }
            if (printCurrentPressKey)
            {
                // SendText(e.KeyCode.ToString().ToLower());
                e.Handled = false;
                Reset();
            }
            
            //m_Events.KeyPress += HookManager_KeyPress;
        }

        public void OnKeyUp(ref object sender, ref KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LShiftKey)
            {
                pressLeftShift = false;
            }
            else if (e.KeyCode == Keys.RShiftKey)
            {
                pressRightShift = false;
            }
            else if (e.KeyCode == Keys.LControlKey
             || e.KeyCode == Keys.RControlKey
             || e.KeyCode == Keys.LWin
             || e.KeyCode == Keys.RWin
             || e.KeyCode == Keys.LMenu
             || e.KeyCode == Keys.RMenu)
            {
                isCtrlAltWinPress = false;
            }
        }

        public void OnMouseClick(ref object sender, ref MouseEventArgs e)
        {
            Reset();
        }

        public void Reset()
        {
            phuAmDau.Clear();
            nguyenAmGiua.Clear();
            nguyenAmGiuaThuHai.Clear();
            phuAmCuoi.Clear();
            dauCuoiCau = Keys.None;
            doubleCuoiCau = Keys.None;
            nguyenAmGiuaBienDoi = new StringBuilder();
            nguyenAmGiuaBienDoiCoDau = new StringBuilder();
            rememberDauCuoiCau = Keys.None;
            // PrintWW = false;
            upperNguyenAm[0] = false;
            upperNguyenAm[1] = false;
            upperNguyenAm[2] = false;
            dauPhuAm = Keys.None;
            isDoubleD = false;
        }

        private StringBuilder ConvertKeysToString(List<Keys> s)
        {
            Keys[] keys = s.ToArray();
            // Array.Reverse(keys);
            string result = "";
            for (int i = 0; i < s.Count; i++)
            {
                result += keys[i].ToString().ToLower();
            }
            return new StringBuilder(result);
        }

        private int KiemTraDau(ref string textSend, Keys key)
        {
            Dictionary<string, string> tDouble = tvu.cDoubleA;
            Dictionary<string, string> tDoubleInvert = tvu.cDoubleAInvert;

            if (key == Keys.O)
            {
                tDouble = tvu.cDoubleO;
                tDoubleInvert = tvu.cDoubleOInvert;
            }
            else if (key == Keys.E)
            {
                tDouble = tvu.cDoubleE;
                tDoubleInvert = tvu.cDoubleEInvert;
            } else if (tvu.IsVNIDouble(key))
            {
                tDouble = tvu.cDouble;
                tDoubleInvert = tvu.cDoubleInvert;
            }
            string nguyenAmGiuaBienDoiString = nguyenAmGiuaBienDoi.ToString().ToLower();
            if (tDouble.ContainsKey(nguyenAmGiuaBienDoiString))
            {
                textSend = tDouble[nguyenAmGiuaBienDoiString];
                nguyenAmGiuaBienDoi = new StringBuilder(textSend);
                return 0;
            }
            if (tDoubleInvert.ContainsKey(nguyenAmGiuaBienDoiString))
            {
                textSend = tDoubleInvert[nguyenAmGiuaBienDoiString];
                nguyenAmGiuaBienDoi = new StringBuilder(textSend);
                return 1;
            }
            return 2;
        }

        private bool KiemTraVekep(ref string textSend, Keys key)
        {
            Dictionary<string,string> cVeKep = tvu.cVekep;
            Dictionary<string, string> cVekepInvert = tvu.cVekepInvert;
            if (tvu.IsVNIDauMocO(key))
            {
                cVeKep = tvu.cVekepOU;
                cVekepInvert = tvu.cVekepInvertOU;
            }
            else if (tvu.IsVNIDauMocA(key))
            {
                cVeKep = tvu.cVekepA;
                cVekepInvert = tvu.cVekepInvertA;
            }
            string nguyenAmGiuaBienDoiString = nguyenAmGiuaBienDoi.ToString().ToLower();
            if (cVeKep.ContainsKey(nguyenAmGiuaBienDoiString))
            {
                textSend = cVeKep[nguyenAmGiuaBienDoiString];
                nguyenAmGiuaBienDoi = new StringBuilder(textSend);
                return true;
            }
            else if (cVekepInvert.ContainsKey(nguyenAmGiuaBienDoiString))
            {
                textSend = cVekepInvert[nguyenAmGiuaBienDoiString];
                nguyenAmGiuaBienDoi = new StringBuilder(textSend);
                return false;
            }
            return false;
        }

        private bool KiemTraDauCuoiCau(ref string textSend, Keys key_press)
        {
            string nguyenAmGiuaBienDoiString = nguyenAmGiuaBienDoi.ToString().ToLower();
            bool check = false;

            int position = 0;
            if (tvu.cDauNguyenAm.ContainsKey(nguyenAmGiuaBienDoiString))
            {
                position = tvu.cDauNguyenAm[nguyenAmGiuaBienDoiString];

                // fix for oa
                if ((nguyenAmGiuaBienDoiString == "oa" || nguyenAmGiuaBienDoiString == "uy")
                    && phuAmCuoi.Count > 0)
                {
                    position = 1;
                }

                string position_char = nguyenAmGiuaBienDoiString[position].ToString();
                int postision_dau = tvu.CheckDau(key_press, isShiftPress());
                if (tvu.cChar.ContainsKey(position_char))
                {
                    if (postision_dau != -1)
                    {

                        if (nguyenAmGiuaBienDoiCoDau.Length != nguyenAmGiuaBienDoi.Length)
                        {
                            nguyenAmGiuaBienDoiCoDau = new StringBuilder(nguyenAmGiuaBienDoi.ToString());
                        }
                        if (nguyenAmGiuaBienDoiCoDau.Length > position &&
                            nguyenAmGiuaBienDoiCoDau[position] == tvu.cChar[position_char][postision_dau][0])
                        {
                            nguyenAmGiuaBienDoiCoDau[position] = position_char[0];
                            textSend = nguyenAmGiuaBienDoiCoDau.ToString();
                            check = false;
                        }
                        else
                        {
                            nguyenAmGiuaBienDoiCoDau[position] = tvu.cChar[position_char][postision_dau][0];
                            textSend = nguyenAmGiuaBienDoiCoDau.ToString();
                            check = true;
                        }
                    }
                    else
                    {
                        // some error
                        check = false;
                    }
                }
            }
            else
            {
                check = false;
            }
            return check;
        }

        // text: ôi
        // dau: Key.J
        // return ội
        private string ThayDoiDoubleCoDau(string text, Keys dau)
        {
            int postision_dau = tvu.CheckDau(dau, isShiftPress());
            StringBuilder result = new StringBuilder(text);

            if (tvu.cDauNguyenAm.ContainsKey(text))
            {
                int pos = tvu.cDauNguyenAm[text];
                string c = text[pos].ToString();
                if (tvu.cChar.ContainsKey(c.ToString()))
                {
                    result[pos] = tvu.cChar[c][postision_dau][0];
                }
            }

            return result.ToString();
        }

        // type = 0 normal
        // type = 1 text la nguyen am giua
        public string SendText(string text, int type = 1)
        {
            // ^ is xor logic
            bool toUpper = isUpperCase();

            if (type == 1)
            {
                StringBuilder textBuilder = new StringBuilder(text);
                for (int i = 0; i < textBuilder.Length && i < 3; i++)
                {
                    if (upperNguyenAm[i])
                    {
                        textBuilder[i] = textBuilder[i].ToString().ToUpper()[0];
                    }
                }
                sim.Keyboard.TextEntry(textBuilder.ToString());
            }
            else if (toUpper)
            {
                text = text.ToUpper();
                sim.Keyboard.TextEntry(text);
            }
            else
            {
                // Fix for Yene -> Yên
                sim.Keyboard.TextEntry(text);
            }

            return text;
        }

        private bool isUpperCase()
        {
            return isCapsLockOn ^ (pressLeftShift || pressRightShift);
        }

        private bool isShiftPress()
        {
            return pressLeftShift || pressRightShift;
        }

        private bool kiegoKhacTelex()
        {
            return kieuGo == "VNI" || kieuGo == "VIQR";
        }

        private bool IsAZKey(Keys key)
        {
            return ! tvu.CheckKeyEndWord(key);
        }

        private bool IsControlKeyCode(Keys key)
        {
            Keys[] listControllKey = new Keys[] {
                Keys.Enter, Keys.LControlKey
                , Keys.RControlKey, Keys.LWin, Keys.RWin, Keys.RMenu, Keys.LMenu
                , Keys.LShiftKey, Keys.RShiftKey, Keys.Back, Keys.Tab


                , Keys.Left , Keys.Right
                , Keys.Down , Keys.Up

                , Keys.F10 , Keys.F1
                , Keys.F2 , Keys.F3
                , Keys.F4 , Keys.F5
                , Keys.F6 , Keys.F7
                , Keys.F8 , Keys.F9
                , Keys.F11 , Keys.F12

                , Keys.Escape , Keys.Delete

                , Keys.PageDown , Keys.Home
                , Keys.PageUp , Keys.End
                , Keys.Scroll , Keys.Pause
                , Keys.Insert , Keys.Next

                , Keys.CapsLock , Keys.Capital
                , Keys.Apps
            };
            if (listControllKey.Contains(key))
            {
                return true;
            }
            return false;
        }
    }
}