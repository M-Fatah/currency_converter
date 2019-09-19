using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DatePicker : MonoBehaviour
{
    [Header("Day")]
    public InputField dayInputField;
    [Header("Month")]
    public InputField monthInputField;
    [Header("Year")]
    public InputField yearInputField;

    private string currentSelectedDay;
    private string currentSelectedMonth;
    private string currentSelectedYear;

    private string currentSelectedDate;

    private DateTime today;

    public System.Action<string> OnDateChanged;

    // TODO: check if date is valid.
    // Date format will be yyyy-mm-dd.

    private void Awake()
    {
        today = DateTime.Today;
        currentSelectedDay = today.Day.ToString();
        currentSelectedMonth = today.Month.ToString();
        currentSelectedYear = today.Year.ToString();

        currentSelectedDate = currentSelectedYear + "-" + currentSelectedMonth + "-" + currentSelectedDay;

        if (dayInputField)
            dayInputField.text = currentSelectedDay;

        if (monthInputField)
            monthInputField.text = currentSelectedMonth;

        if (yearInputField)
            yearInputField.text = currentSelectedYear;
    }

    /// <summary>
    /// Increments/Decrements day value then update day input field with the new value.
    /// </summary>
    /// <param name="direction"> Used to determine if we want to increment (+1) or decrement (-1). </param>
    public void DayButton(int direction)
    {
        int d = int.Parse(currentSelectedDay);
        d += direction;

        if (d < 1)
            d = 1;

        if (d > 31)
            d = 31;

        currentSelectedDay = d.ToString("D2");

        if (dayInputField)
            dayInputField.text = currentSelectedDay;

        OnDateChanged?.Invoke(GetDate());
    }

    /// <summary>
    /// Increments/Decrements month value then update month input field with the new value.
    /// </summary>
    /// <param name="direction"> Used to determine if we want to increment (+1) or decrement (-1). </param>
    public void MonthButton(int direction)
    {
        int m = int.Parse(currentSelectedMonth);
        m += direction;

        if (m < 1)
            m = 1;

        if (m > 12)
            m = 12;

        currentSelectedMonth = m.ToString("D2");

        if (monthInputField)
            monthInputField.text = currentSelectedMonth;

        OnDateChanged?.Invoke(GetDate());
    }

    /// <summary>
    /// Increments/Decrements year value then update year input field with the new value.
    /// </summary>
    /// <param name="direction"> Used to determine if we want to increment (+1) or decrement (-1). </param>
    public void YearButton(int direction)
    {
        int y = int.Parse(currentSelectedYear);
        y += direction;

        if (y < 1999)
            y = 1999;

        if (y > today.Year)
            y = today.Year;

        currentSelectedYear = y.ToString("D4");

        if (yearInputField)
            yearInputField.text = currentSelectedYear;

        OnDateChanged?.Invoke(GetDate());
    }

    /// <summary>
    /// Updates day input field with the new value;
    /// </summary>
    /// <param name="v"> The update value </param>
    public void OnDayInputChanged(string v)
    {
        if (string.IsNullOrEmpty(v) || int.Parse(v) == 0)
            currentSelectedDay = today.Day.ToString("D2");
        
        currentSelectedDay = v;

        if (dayInputField)
            dayInputField.text = currentSelectedDay;

        OnDateChanged?.Invoke(GetDate());
    }

    /// <summary>
    /// Updates month input field with the new value;
    /// </summary>
    /// <param name="v"> The update value </param>
    public void OnMonthInputChanged(string v)
    {
        if (string.IsNullOrEmpty(v) || int.Parse(v) == 0)
            currentSelectedMonth = today.Month.ToString("D2");

        currentSelectedMonth = v;

        if (monthInputField)
            monthInputField.text = currentSelectedMonth;

        OnDateChanged?.Invoke(GetDate());
    }

    /// <summary>
    /// Updates year input field with the new value;
    /// </summary>
    /// <param name="v"> The update value </param>
    public void OnYearInputChanged(string v)
    {
        if (string.IsNullOrEmpty(v) || int.Parse(v) == 0)
            currentSelectedYear = today.Year.ToString("D4");

        currentSelectedYear = v;

        if (yearInputField)
            yearInputField.text = currentSelectedYear;

        OnDateChanged?.Invoke(GetDate());
    }

    /// <summary>
    /// Returns the current date choosen in the date picker.
    /// </summary>
    /// <returns></returns>
    public string GetDate()
    {
        currentSelectedDate = currentSelectedYear + "-" + currentSelectedMonth + "-" + currentSelectedDay;

        return currentSelectedDate;
    }
}
