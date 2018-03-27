using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace n1mm2web
{
    /// <summary>
    /// The program sends Radio Info packets at 10 second intervals, or immediately after an event where any of the information in one of these fields changes (for example: user changes bands, spins the VFO dial, switches from Run to S&P, or selects VFOb).
    /// </summary>
    [XmlRoot(ElementName = "RadioInfo")]
    public class N1mmRadioInfo
    {
        /// <summary>
        /// StationName is the NetBios name of the computer that is sending these messages. It is the name used in Mulit-Computer networking. Windows limits it to 15 characters. If the computer name is greater than 15 characters long, the first 15 characters will be used.
        /// </summary>
        [XmlElement(ElementName = "StationName")]
        public string StationName { get; set; }

        /// <summary>
        /// RadioNr is the radio number associated with a specific XML packet - in other words, the source of the information in that packet. When in SO2V or SO2R mode, N1MM+ sends two packets every ten seconds - one packet each from RadioNr1 and RadioNr2
        /// </summary>
        [XmlElement(ElementName = "RadioNr")]
        public int RadioNr { get; set; }

        /// <summary>
        /// Freq_dHz is the receive frequency represented as values to the tens digit with no delimiter. For example: 160 meters: 181234; 40 meters: 712345; 10 meters: 2812345; 6 meters: 5012345.
        /// This is what comes out of N1MM.
        /// </summary>
        [XmlElement(ElementName = "Freq")]
        public int Freq_dHz { get; set; }

        /// <summary>
        /// TXFreq_dHz is the transmit frequency represented as values to the tens digit with no delimiter. For example: 160 meters: 181234; 40 meters: 712345; 10 meters: 2812345; 6 meters: 5012345.
        /// This is what comes out of N1MM.
        /// </summary>
        [XmlElement(ElementName = "TXFreq")]
        public int TXFreq_dHz { get; set; }

        /// <summary>
        /// Mode could be any one of the following: CW, USB, LSB, RTTY, PSK31, PSK63, PSK125, PSK250, QPSK31, QPSK63, QPSK125, QPSK250, FSK, GMSK, DOMINO, HELL, FMHELL, HELL80, MT63, THOR, THRB, THRBX, OLIVIA, MFSK8, MFSK16
        /// </summary>
        [XmlElement(ElementName = "Mode")]
        public string Mode { get; set; }

        /// <summary>
        /// OpCall is the callsign entered by the operator after OPON (or Ctl-O). Defaults to the station call
        /// </summary>
        [XmlElement(ElementName = "OpCall")]
        public string OpCall { get; set; }

        [XmlElement(ElementName = "IsRunning")]
        private string IsRunning_str { get; set; }

        /// <summary>
        /// IsRunning represents the value of the RUN &lt;&gt; S&P radio buttons in the Entry Window. If you are on your CQ frequency, IsRunning will be True
        /// </summary>
        public bool IsRunning { get { return IsRunning_str == "True"; } }

        /// <summary>
        /// FocusEntry is the Windows assigned handle of the Entry Window with program focus
        /// </summary>
        [XmlElement(ElementName = "FocusEntry")]
        public int FocusEntry { get; set; }

        /// <summary>
        /// Antenna is the currently selected antenna for this radio (0-15), from the Antenna tab in the Configurer
        /// </summary>
        [XmlElement(ElementName = "Antenna")]
        public int Antenna { get; set; }

        /// <summary>
        /// Rotors is the name of the currently selected rotor from the Antenna table in the Configurer
        /// </summary>
        [XmlElement(ElementName = "Rotors")]
        public string Rotors { get; set; }

        /// <summary>
        /// FocusRadioNr Receive Radio Focus - the Green Dot in the Entry Window. Enables signal switching for SO2R operation - facilitating the routing of Microphone, Audio, PTT, CW signals to/from the selected radio. FocusRadioNr toggles between 1 and 2 when the program, a mouse click, or the \ key selects the opposite Entry Window
        /// </summary>
        [XmlElement(ElementName = "FocusRadioNr")]
        public int FocusRadioNr { get; set; }

        [XmlElement(ElementName = "IsStereo")]
        private string IsStereo_str { get; set; }

        /// <summary>
        /// IsStereo enables audio switching for SO2R operation.The ` key (backquote) toggles its value between True and False
        /// </summary>
        public bool IsStereo { get { return IsStereo_str == "True"; } }

        /// <summary>
        /// ActiveRadioNr Transmit Radio Focus - the Red Dot in the Entry Window. Enables signal switching for SO2R operation - facilitating the routing of Microphone, Audio, PTT, CW signals to/from the selected radio. ActiveRadioNr toggles between 1 and 2 when the program or pressing the &lt;pause&gt; key selects the opposite Entry Window
        /// </summary>
        [XmlElement(ElementName = "ActiveRadioNr")]
        public int ActiveRadioNr { get; set; }

        public static bool TryParse(byte[] datagram, out N1mmRadioInfo radioInfo)
        {
            string str;
            try
            {
                str = Encoding.UTF8.GetString(datagram);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception: {0}", ex));
                radioInfo = null;
                return false;
            }

            try
            {
                var serialiser = new XmlSerializer(typeof(N1mmRadioInfo));
                using (var reader = new StringReader(str))
                {
                    radioInfo = (N1mmRadioInfo)serialiser.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception: {0}", ex));
                radioInfo = null;
                return false;
            }

            return true;
        }
    }
}
