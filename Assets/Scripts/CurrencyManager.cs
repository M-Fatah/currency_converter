using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
    [Header("Currency 1")]
    public InputField inputField1;
    public Dropdown currencyDrpDwn1;
    [Header("Currency 2")]
    public InputField inputField2;
    public Dropdown currencyDrpDwn2;
    [Header("Date picker")]
    public DatePicker datePicker;


    private string[] currencyNames = {"USD", "CAD", "GBP", "HKD",  
                                      "HUF", "CZK", "AUD", "RON", "SEK",
                                      "IDR", "INR", "BRL", "RUB", "HRK",
                                      "JPY", "THB", "CHF", "SGD", "PLN",
                                      "BGN", "TRY", "CNY", "NOK", "NZD",
                                      "ZAR", "PHP", "MXN", "ILS", "ISK",
                                      "KRW", "MYR", "DKK",};

    private Currency currency;

    private int baseCurrencyIndex = 0;
    private int altCurrencyIndex = 0;

    private float baseCurrencyAmount = 1;
    private float altCurrencyAmount = 0;

    private Dictionary<string, string> queriedData;

    void Start()
    {
        currency = new Currency();
        currency.rates = new Rate[currencyNames.Length];
        queriedData = new Dictionary<string, string>();
        
        // Populate Dropdowns with array data at startup.
        if(currencyDrpDwn1 & currencyDrpDwn2)
        {
            currencyDrpDwn1.ClearOptions();
            currencyDrpDwn2.ClearOptions();

            for (int i = 0; i < currencyNames.Length; i++)
            {
                currencyDrpDwn1.options.Add(new Dropdown.OptionData(currencyNames[i]));
                currencyDrpDwn2.options.Add(new Dropdown.OptionData(currencyNames[i]));
            }
        }

        // Get previously chosen currencies if there is any otherwise choose default values.
        baseCurrencyIndex = PlayerPrefs.GetInt("CURR1", 1);
        altCurrencyIndex = PlayerPrefs.GetInt("CURR2", 2);

        currencyDrpDwn1.value = baseCurrencyIndex;
        currencyDrpDwn2.value = altCurrencyIndex;


        OnDrpDwn1ValueChanged(baseCurrencyIndex);
        OnDrpDwn2ValueChanged(altCurrencyIndex);

        if (datePicker)
            datePicker.OnDateChanged += OnDateChanged;
    }

    /// <summary>
    /// Gets new currency rates from the date specified.
    /// </summary>
    /// <param name="newDate"></param>
    void OnDateChanged(string newDate)
    {
        GetCurrencyRates(newDate);

        altCurrencyAmount = Calculate(baseCurrencyAmount, altCurrencyIndex);

        if (inputField1)
        {
            inputField1.text = baseCurrencyAmount.ToString();
        }

        if (inputField2)
        {
            inputField2.text = altCurrencyAmount.ToString();
        }
    }

    /// <summary>
    /// Called whenever the user changes the value on currency1 dropdown.
    /// </summary>
    /// <param name="v"> The new dropdown index of the value chosen. </param>
    public void OnDrpDwn1ValueChanged(int v)
    {
        PlayerPrefs.SetInt("CURR1", v);
        PlayerPrefs.Save();

        baseCurrencyIndex = v;

        GetCurrencyRates(datePicker.GetDate());

        altCurrencyAmount = Calculate(baseCurrencyAmount, altCurrencyIndex);

        if (inputField1)
        {
            inputField1.text = baseCurrencyAmount.ToString();
        }

        if (inputField2)
        {
            inputField2.text = altCurrencyAmount.ToString();
        }
    }

    /// <summary>
    /// Called whenever the user changes the value on currency2 dropdown.
    /// </summary>
    /// <param name="v"> The new dropdown index of the value chosen. </param>
    public void OnDrpDwn2ValueChanged(int v)
    {
        PlayerPrefs.SetInt("CURR2", v);
        PlayerPrefs.Save();

        altCurrencyIndex = v;

        altCurrencyAmount = Calculate(baseCurrencyAmount, altCurrencyIndex);

        if (inputField1)
        {
            inputField1.text = baseCurrencyAmount.ToString();
        }

        if (inputField2)
        {
            inputField2.text = altCurrencyAmount.ToString();
        }

    }

    /// <summary>
    /// Called whenever the user inputs a value on currency1 InputField.
    /// </summary>
    /// <param name="t"> the value of the user input. </param>
    public void OnInput1ValueChanged(string t)
    {
        baseCurrencyAmount = float.Parse(t);

        altCurrencyAmount = Calculate(baseCurrencyAmount, altCurrencyIndex);

        if (inputField1)
        {
            inputField1.text = baseCurrencyAmount.ToString();
        }

        if (inputField2)
        {
            inputField2.text = altCurrencyAmount.ToString();
        }
    }

    /// <summary>
    /// Called whenever the user inputs a value on currency2 InputField.
    /// </summary>
    /// <param name="t"> the value of the user input. </param>
    public void OnInput2ValueChanged(string t)
    {
        altCurrencyAmount = float.Parse(t);

        baseCurrencyAmount = Calculate(altCurrencyAmount / currency.rates[altCurrencyIndex].value, baseCurrencyIndex);

        if (inputField1)
        {
            inputField1.text = baseCurrencyAmount.ToString();
        }

        if (inputField2)
        {
            inputField2.text = altCurrencyAmount.ToString();
        }
    }

    /// <summary>
    /// Swaps currency values.
    /// Called when pressing the swap button.
    /// </summary>
    public void SwapCurrencies()
    {
        if(currencyDrpDwn1 && currencyDrpDwn2)
        {
            int tmp = currencyDrpDwn1.value;
            currencyDrpDwn1.value = currencyDrpDwn2.value;
            currencyDrpDwn2.value = tmp;

            OnDrpDwn1ValueChanged(currencyDrpDwn1.value);
            OnDrpDwn2ValueChanged(currencyDrpDwn2.value);
        }
    }

    /// <summary>
    /// Calculate the money amount when converting from a currency to another.
    /// </summary>
    /// <param name="currencyAmount"> The money amount to convert. </param>
    /// <param name="rateIndex"> the rate of the currency that we want to convert to. </param>
    /// <returns></returns>
    float Calculate(float currencyAmount, int rateIndex)
    {
        if (currency.rates[rateIndex] == null)
            return 0;

        return currencyAmount * currency.rates[rateIndex].value;
    }

    /// <summary>
    /// Query currency rates from @https://api.exchangeratesapi.io
    /// </summary>
    /// <param name="baseCurrency"> The currency we want to convert from. </param>
    void GetCurrencyRates(string date)
    {
        // Check if data is already queried once before.
        string key = currencyNames[baseCurrencyIndex] + date;

        // Check if the data is contained in our dictionary, then get it otherwise query new data from the internet.
        if (queriedData.ContainsKey(key))
        {
            // Parse json data.
            JObject o = JObject.Parse(queriedData[key]);

            currency.name = o["base"].Value<string>();
            currency.date = o["date"].Value<DateTime>();

            for (int i = 0; i < currency.rates.Length; i++)
            {
                string name = currencyNames[i];

                Rate rate = new Rate();
                rate.name = name;
                rate.value = o["rates"][name].Value<float>();
                currency.rates[i] = rate;
#if UNITY_EDITOR
                Debug.Log(name + " : " + o["rates"][name].Value<float>());
#endif
            }
#if UNITY_EDITOR

            Debug.Log("\n-----------------------------------------------\n");

            Debug.Log("Used data queried once before");
#endif
        }
        else
        {
            string URL = @"https://api.exchangeratesapi.io/latest?base=" + currencyNames[baseCurrencyIndex];

            if (datePicker)
            {
#if UNITY_EDITOR

                Debug.Log("Querying Data as of Date: " + datePicker.GetDate());
#endif
                URL = @"https://api.exchangeratesapi.io/" + datePicker.GetDate() + "?base=" + currencyNames[baseCurrencyIndex];
            }

            UnityWebRequest ww = new UnityWebRequest(URL);

            ww.method = "GET";

            ww.SetRequestHeader("Content-Type", "plain/text");

            ww.downloadHandler = new DownloadHandlerBuffer();

            ww.SendWebRequest();

            while (!ww.isDone);

            if (!string.IsNullOrEmpty(ww.error))
            {
                Debug.Log("Error: getting API request failed, " + ww.error);

                return;
            }

            //currency = JsonUtility.FromJson<Currency>(ww.downloadHandler.text);


            // Cache data for later use.
            if (!queriedData.ContainsKey(key))
                queriedData.Add(key, ww.downloadHandler.text);

            // Parse json data.
            JObject o = JObject.Parse(ww.downloadHandler.text);

            currency.name = o["base"].Value<string>();
            currency.date = o["date"].Value<DateTime>();

            for (int i = 0; i < currency.rates.Length; i++)
            {
                string name = currencyNames[i];

                Rate rate = new Rate();
                rate.name = name;
                rate.value = o["rates"][name].Value<float>();
                currency.rates[i] = rate;
#if UNITY_EDITOR
                Debug.Log(name + " : " + o["rates"][name].Value<float>());
#endif
            }
            //Debug.Log("\n-----------------------------------------------\n");

#if false
            // TODO: Save data to be persistant for a whole day then update the next day.

            PlayerPrefs.SetString("DATE", currency.date.ToString());
            PlayerPrefs.SetString("DATA", ww.downloadHandler.text);
            PlayerPrefs.Save();
#endif
        }
    }

    [System.Serializable]
    public class Currency
    {
        public Rate[] rates;
        public string name;
        public DateTime date;
    }

    [System.Serializable]
    public class Rate
    {
        public string name;
        public float value;
    }
}
