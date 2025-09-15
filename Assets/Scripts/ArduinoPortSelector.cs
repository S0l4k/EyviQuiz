using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;
using System.Collections;
using TMPro;

public class ArduinoPortSelector : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField manualPortField; 
    public Button checkButton;             
    public Image connectionStatus;         
    public Color connectedColor = Color.green;
    public Color disconnectedColor = Color.red;

    public ArduinoInputManager arduinoInputManager;

    private SerialPort tempPort;

    void Start()
    {
        
        connectionStatus.color = disconnectedColor;
        checkButton.onClick.AddListener(OnCheckPort);
    }

    
    void OnCheckPort()
    {
        string manualPort = manualPortField.text.Trim();
        if (string.IsNullOrEmpty(manualPort))
        {
            Debug.LogWarning("Wpisz port szeregowy np. COM3.");
            return;
        }
        StartCoroutine(CheckArduinoConnection(manualPort));
    }

   
    IEnumerator CheckArduinoConnection(string portName)
    {
        tempPort = new SerialPort(portName, 9600);
        try
        {
            tempPort.Open();
            tempPort.ReadTimeout = 1000;
            tempPort.WriteLine("check");
        }
        catch (System.Exception e)
        {
            Debug.LogError("B³¹d otwierania portu: " + e.Message);
            connectionStatus.color = disconnectedColor;
            yield break;
        }

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

        tempPort.Close();

        if (okReceived)
        {
            Debug.Log("Po³¹czono z Arduino!");
            connectionStatus.color = connectedColor;

            if (arduinoInputManager != null)
            {
                
                arduinoInputManager.UseExternalPort(new SerialPort(portName, 9600), portName);
            }
        }
        else
        {
            Debug.LogWarning("Brak odpowiedzi od Arduino.");
            connectionStatus.color = disconnectedColor;
        }
    }
}
