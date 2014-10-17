using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data;


namespace MonitoringServer.DataForServer.Ivr
{
    public class LinesStatus
    {
        public static LinesStatus staticLineStatus;
        /// <summary>
        /// Статичные переменные.
        /// </summary>
        public static bool staticCanUpdateGetData; // параметр указывает на возможность и невозможность забрать данные из ivrStatusTable.
        public static Hashtable staticIvrStatusTable; // таблица, содержащая объекты IvrData и ключи с IVRID.
        
        /// <summary>
        /// Динамичные переменные.
        /// </summary>
        public LinesStatus lineStatus 
        { get { if (staticLineStatus == null) { staticLineStatus = new LinesStatus(); }
            return staticLineStatus; } }

        public bool canGetData { get { return staticCanUpdateGetData; } set { staticCanUpdateGetData = value; } }
        public Hashtable ivrStatusTable { get { return staticIvrStatusTable; } set { staticIvrStatusTable = value; } }
       


        /// <summary>
        /// Получить хеш таблицу с информацией по линиям IVR.
        /// </summary>
        /// <returns>Hashtable key = IVRID, value = IvrData</returns>
        public Hashtable getData()
        {
            Hashtable tempSyncTable 
                = Hashtable.Synchronized(lineStatus.ivrStatusTable);
            return tempSyncTable;
        }
        /// <summary>
        /// Обновляем объект 
        /// </summary>
        /// <param name="table"></param>
        public void updateData(Hashtable table)
        {
            lineStatus.ivrStatusTable = table;
        }
    }
}
