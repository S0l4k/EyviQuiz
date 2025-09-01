using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;
using System.Collections;
using TMPro;

public class ArduinoPortSelector : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown portDropdown;
    public Button connectButton;
    public Image connectionStatus; 
    public Color connectedColor = Color.green;
    public Color disconnectedColor = Color.red;

    public ArduinoInputManager arduinoInputManager;

    private SerialPort tempPort;
    private string[] availablePorts;

    void Start()
    {
        connectionStatus.color = disconnectedColor;
        RefreshPorts();
        connectButton.onClick.AddListener(OnConnect);
    }

    void RefreshPorts()
    {
#if UNITY_STANDALONE_WIN
        availablePorts = SerialPort.GetPortNames();
        if (availablePorts == null || availablePorts.Length == 0)
        {
            availablePorts = new string[] { "No Ports" };
        }
#else
        availablePorts = new string[] { "N/A" };
#endif
        portDropdown.ClearOptions();
        portDropdown.AddOptions(new System.Collections.Generic.List<string>(availablePorts));
    }

    void OnConnect()
    {
        string selectedPort = availablePorts[portDropdown.value];
        if (selectedPort == "No Ports" || selectedPort == "N/A")
        {
            Debug.LogWarning("Nie ma dostêpnych portów!");
            return;
        }

        tempPort = new SerialPort(selectedPort, 9600);
        try
        {
            tempPort.Open();
            tempPort.ReadTimeout = 1000; 
            StartCoroutine(CheckArduinoConnection(selectedPort));
        }
        catch (System.Exception e)
        {
            Debug.LogError("B³¹d otwierania portu: " + e.Message);
        }
    }

    IEnumerator CheckArduinoConnection(string portName)
    {
        if (tempPort != null && tempPort.IsOpen)
        {
            tempPort.WriteLine("check");
            float timer = 0f;
            bool okReceived = false;

            while (timer < 3f)
            {
                timer += Time.deltaTime;
                try
                {
                    string response = tempPort.ReadLine().Trim();
                    if (response == "OK")
                    {
                        okReceived = true;
                        break;
                    }
                }
                catch { }
                yield return null;
            }

            if (okReceived)
            {
                Debug.Log("Po³¹czono z Arduino!");
                connectionStatus.color = connectedColor;

                
                if (arduinoInputManager != null)
                {
                    arduinoInputManager.portName = portName;
                    arduinoInputManager.OpenPort(); 
                }

                tempPort.Close();
            }
            else
            {
                Debug.LogWarning("Brak odpowiedzi od Arduino.");
                connectionStatus.color = disconnectedColor;
                tempPort.Close();
            }
        }
    }
}
