﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WestLakeShape.Motion;
using WestLakeShape.Motion.Device;
using static WestLakeShape.Motion.IOStateSource;

namespace NanoImprinter.Model
{
    public class ImprinterIO
    {
        //private const string ConfigFileName = "//Config//GTSIO.config";


        //public static bool LoadIOConfig(string rootFolder)
        //{

        //    return true;
        //}

        private ImprinterIOConfig _config;
        private TrioIOStateSource _ioSource;


        public ImprinterIOConfig Config => _config;


        public ImprinterIO(ImprinterIOConfig config)
        {
            _config = config;
            LoadIOState();
            _ioSource = new TrioIOStateSource(_config.IOStateSourceConfig);
        }

        /// <summary>
        /// 添加新的IO
        /// </summary>
        private void LoadIOState()
        {
            var enumValues = Enum.GetValues(typeof(ImprinterIOName)).Cast<ImprinterIOName>();
            foreach (var enumValue in enumValues)
            {
                if (!_config.IOStateSourceConfig.States.Any(o => o.Name == enumValue.ToString()))
                {
                    _config.IOStateSourceConfig.States.Add(new IOStateConfig()
                    {
                        Name = enumValue.ToString()
                    });
                }
            }
        }

        public IOState GetInputIO(ImprinterIOName name)
        {
            return _ioSource.InputStates[name.ToString()];
        }
    }

    public class ImprinterIOConfig
    {
        public string Name { get; set; }

        public IOStateSourceConfig IOStateSourceConfig { get; set; }
    }


    public enum ImprinterIOName
    {
        SaftDoor,
        LoadWafeDoor,
        FixedWafe,
        HasWafe,
        FixedMark
    }
}