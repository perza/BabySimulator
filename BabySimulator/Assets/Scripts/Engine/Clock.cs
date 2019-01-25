
/// <summary>
/// Barn clock.
/// 
/// This clock does not take leaping years and other real calendar related things into account.
/// Months are based on western 12 month calendar.
/// </summary>
internal class Clock : PersistentEngineSingleton<Clock>
{
    double m_CurrentBarnTime; // time from simulation start in seconds

    Date m_SimulationStartingDate;
    Date m_CurrentDate;
    public Date SimulationStartingDate { get { return m_SimulationStartingDate.Clone(); } }
    public Date CurrentDate { get { UpdateDateTime();  return m_CurrentDate.Clone(); } }

    Time m_SimulationStartingTime;
    Time m_CurrentTime;
    public Date SimulationStartingTime { get { UpdateDateTime();  return m_SimulationStartingDate.Clone(); } }
    public Time CurrentTime { get { return m_CurrentTime.Clone(); } }

    /// <summary>
    /// Player can set the starting date and time to simulate seasons
    /// </summary>
    /// <param name="starting_date"></param>
    /// <param name="starting_time"></param>
    public Clock (Date starting_date = null, Time starting_time = null)
    {
        
        if (null == starting_date)
        {
            m_SimulationStartingDate = new Date();

            m_SimulationStartingDate.m_Year =  System.DateTime.Now.Year;
            m_SimulationStartingDate.m_Month = System.DateTime.Now.Month;
            m_SimulationStartingDate.m_Day = System.DateTime.Now.Day;

        }
        else
        {
            m_SimulationStartingDate = starting_date.Clone();
        }

        m_CurrentDate = m_SimulationStartingDate.Clone();
        m_CurrentBarnTime = ConvertDateToSeconds(m_CurrentDate);


        if (null == starting_time)
        {
            m_SimulationStartingTime = new Time();

            m_SimulationStartingTime.m_Hour = System.DateTime.Now.Hour;
            m_SimulationStartingTime.m_Minute = System.DateTime.Now.Minute;
            m_SimulationStartingTime.m_Second = System.DateTime.Now.Second;
        }
        else
        {
            m_SimulationStartingTime = starting_time.Clone();
        }

        m_CurrentTime = m_SimulationStartingTime.Clone();
        m_CurrentBarnTime += ConvertTimeToSeconds(m_CurrentTime);
    }

    double GetSecInMonth(int month)
    {
        if (month == 12) return (double)334 * 24 * 3600;
        else if (month == 11) return (double)304 * 24 * 3600;
        else if (month == 10) return (double)273 * 24 * 3600;
        else if (month == 9) return (double)243 * 24 * 3600;
        else if (month == 8) return (double)212 * 24 * 3600;
        else if (month == 7) return (double)181 * 24 * 3600;
        else if (month == 6) return (double)151 * 24 * 3600;
        else if (month == 5) return (double)120 * 24 * 3600;
        else if (month == 4) return (double)90 * 24 * 3600;
        else if (month == 3) return (double)59 * 24 * 3600;
        else if (month == 2) return (double)31 * 24 * 3600;
        else return 0;
    }


    double ConvertDateToSeconds(Date date)
    {
        double res = 0;

        res = (double) (date.m_Year-1) * 365 * 24 * 3600;
        res += GetSecInMonth(date.m_Month);
        res += (double) (date.m_Day-1) * 24 * 3600;

        return res;
    }
    double ConvertTimeToSeconds(Time time)
    {
        double res = 0;

        res += (double) time.m_Hour * 3600;
        res += (double) time.m_Minute * 60;
        res += time.m_Second;

        return res;
    }


    public void Update ()
    {
        m_CurrentBarnTime += GameManager.m_Instance.m_GameDeltaTime;
    }

    /// <summary>
    ///  Do not call this every frame or second, but once per minute at max! 
    /// </summary>
    void UpdateDateTime ()
    {
        double remains=0.0;

        int years = (int) (m_CurrentBarnTime / (365.0 * 24.0 * 3600.0)) + 1; // years start from 1
        remains = m_CurrentBarnTime - (years-1) * (365.0 * 24.0 * 3600.0);
        int days = (int)(remains / (24.0 * 3600.0));
        month months = GetMonth(days);
        days = (int)months.remainder_days + 1; // days start from 1
        remains -= months.remainder_days * 24.0 * 3600.0;
        int hours = (int)(remains / 3600.0);
        remains -= hours * 3600.0;
        int minutes = (int)(remains / 60.0);
        remains -= minutes * 60.0;
        int seconds = (int)remains;

        m_CurrentDate.m_Year = years;
        m_CurrentDate.m_Month = months.months;
        m_CurrentDate.m_Day = days;

        m_CurrentTime.m_Hour = hours;
        m_CurrentTime.m_Minute = minutes;
        m_CurrentTime.m_Second = seconds;
    }

    class month
    {
        public int months;
        public int remainder_days;
        public month (int m, int rd)
        {
            months = m; remainder_days = rd;
        }
    }

    month GetMonth(int days)
    {
        if (days > 334) return new month (12, days- 334);
        else if (days > 304) return new month(11, days - 304); 
        else if (days > 273) return new month(10, days - 273);
        else if (days > 243) return new month(9, days - 243);
        else if (days > 212) return new month(8, days - 212);
        else if (days > 181) return new month(7, days - 181);
        else if (days > 151) return new month(6, days - 151);
        else if (days > 120) return new month(5, days - 120);
        else if (days > 90) return new month(4, days - 90);
        else if (days > 59) return new month(3, days - 59);
        else if (days > 31) return new month(2, days - 31);
        else return new month(1, days);
    }

    public class Date
    {
        public int m_Day;
        public int m_Month;
        public int m_Year;

        public Date Clone ()
        {
            Date new_date = new Date();
            new_date.m_Day = m_Day;
            new_date.m_Month = m_Month;
            new_date.m_Year = m_Year;
            return new_date;
        }
    }

    public class Time
    {
        public int m_Hour;
        public int m_Minute;
        public int m_Second;

        public Time Clone()
        {
            Time new_date = new Time();
            new_date.m_Hour = m_Hour;
            new_date.m_Minute = m_Minute;
            new_date.m_Second = m_Second;
            return new_date;
        }

    }

}