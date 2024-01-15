﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WestLakeShape.Motion
{
    public abstract class IOStateSource:Connectable
    {
        private IOStateSourceConfig _config;
        private List<IOState> _outputIOs;
        private byte[] _tempBuffer;

        protected byte[] _inputBuffer;
        protected byte[] _outputBuffer;
        protected byte[] _dirtyMasks;

        public Dictionary<string, IOState> InputStates { get; private set; }
        public Dictionary<string, IOState> OutputStates { get; private set; }


        public override string Name => _config.Name;

        public byte[] InputBuffer => _inputBuffer;
        public byte[] OutputBuffer => _outputBuffer;
        
        public IOStateSourceConfig Config => _config;

        public IOStateSource(IOStateSourceConfig config)
        {
            _config = config;
            
            _inputBuffer = new byte[_config.InputBufferLength];
            _outputBuffer = new byte[_config.OutputBufferLength];
            _dirtyMasks = new byte[_config.OutputBufferLength];
            _tempBuffer = new byte[_config.OutputBufferLength];

            InputStates = new Dictionary<string, IOState>();
            OutputStates = new Dictionary<string, IOState>();
        }

        protected override void OnConnecting() 
        {
            LoadStates();
        }

        private void LoadStates()
        {
            foreach (var config in _config.States)
            {
                var state = new IOState(config, this);
                if (config.Type == IOType.Input)
                    InputStates.Add(state.Name, state);
                else
                    OutputStates.Add(state.Name, state);
            }
            _outputIOs = OutputStates.Values.Cast<IOState>().ToList();
        }

        protected override void RefreshStates()
        {
            RefreshOutputs();
            WriteOutputs(_outputBuffer);

            Array.Clear(_dirtyMasks, 0, _dirtyMasks.Length);
            _outputIOs.ForEach(o => o.HasChanged());
            ReadInputs(_inputBuffer);
        }

        private void RefreshOutputs()
        {
            ReadOutputs(_tempBuffer);
            for (var i = 0; i < _outputBuffer.Length; i++)
            {
                var mask = _dirtyMasks[i];
                var remote = _tempBuffer[i];
                var local = _outputBuffer[i];
                var merged = (byte)(_tempBuffer[i] ^ ((remote ^ local) & mask));
                _outputBuffer[i] = merged;
            }
        }

        protected abstract bool ReadInputs(byte[] buffer);

        /// <summary>
        /// 读取输出状态到缓冲区
        /// </summary>
        protected abstract bool ReadOutputs(byte[] buffer);

        /// <summary>
        /// 从缓冲区写入输出状态
        /// </summary>
        protected abstract bool WriteOutputs(byte[] buffer);


        public bool State(string name)
        {
            return InputStates[name].Get();
        }

        public class IOState : BaseState
        {
            private readonly IOStateConfig _config;
            private readonly IOStateSource _source;

            public override bool ReadOnly => _config.Type == IOType.Input;


            public IOState(IOStateConfig config, IOStateSource ioSource) : base(config.Name)
            {
                _config = config;
                _source = ioSource;
            }


            public override bool Get()
            {
                var buffer = _config.Type == IOType.Input
                    ? _source.InputBuffer
                    : _source.OutputBuffer;

                var data = buffer[_config.ByteIndex];
                return 0 != (data & (1 << _config.BitIndex));
            }


            public override void Set(bool value)
            {
                if (_config.Type == IOType.Input)
                    throw new InvalidOperationException("输入状态不允许写入");

                var buffer = _source.OutputBuffer;
                var data = buffer[_config.ByteIndex];
                if (value)
                {
                    data |= (byte)(1 << _config.BitIndex);
                }
                else
                {
                    data &= (byte)~(1 << _config.BitIndex);
                }
                buffer[_config.ByteIndex] = data;

                var masks = _source._dirtyMasks;
                var mask = masks[_config.ByteIndex];
                mask |= (byte)(1 << _config.BitIndex);
                masks[_config.ByteIndex] = mask;
                Dirty = true;
            }
        }
    }

    public class IOStateSourceConfig
    {
        [Category("程序"), Description("状态包名称，如：IO卡1")]
        [DisplayName("状态包名称"), Required, StringLength(32)]
        public string Name { get; set; } = "";


        [Category("I/O"), Description("输入缓冲区字节数"), DefaultValue(1)]
        [DisplayName("输入缓冲区字节数"), Range(0, 256)]
        public int InputBufferLength { get; set; } = 1;


        [Category("I/O"), Description("输出缓冲区字节数"), DefaultValue(1)]
        [DisplayName("输出缓冲区字节数"), Range(0, 256)]
        public int OutputBufferLength { get; set; } = 1;


        [Category("IO口"), Description("IO口")]
        [DisplayName("IO口")]
        public Collection<IOStateConfig> States { get; set; } = new Collection<IOStateConfig>();

    }


    public class IOStateConfig
    {
        [Category("I/O"), Description("IO名称")]
        [DisplayName("IO名称"), Required, StringLength(32)]
        public string Name { get; set; } = "";

        /// <summary>
        /// 是否输入状态。True 代表输入状态，False 代表输出状态
        /// <para>输入状态只读，输出状态可读写</para>
        /// </summary>
        [Category("I/O"), Description("状态类别，输入还是输出"), DefaultValue(IOType.Input)]
        [DisplayName("状态类别")]
        public IOType Type { get; set; } = IOType.Input;

        /// <summary>
        /// 对应 Buffer 中存储字节索引
        /// </summary>
        [Category("I/O"), Description("字节索引。该状态存储于对应缓冲区第几字节，基于 0"), DefaultValue(0)]
        [DisplayName("字节索引"), Range(0, 255)]
        public int ByteIndex { get; set; } = 0;

        /// <summary>
        /// 对应 Buffer 中存储字节中的位(Bit)索引
        /// </summary>
        [Category("I/O"), Description("字位索引。该状态存储于所在字节的第几位，基于 0"), DefaultValue(0)]
        [DisplayName("字位索引"), Range(0, 7)]
        public int BitIndex { get; set; } = 0;

        /// <summary>
        /// 有效电平
        /// </summary>
        [Category("I/O"), Description("有效电平,例如低电平有效"), DefaultValue(IOActiveLevel.Lower)]
        [DisplayName("有效电平"), StringLength(128)]
        public IOActiveLevel ActiveLevel { get; set; } = IOActiveLevel.Lower;

        /// <summary>
        /// 说明
        /// </summary>
        [Category("其他"), Description("状态说明")]
        [DisplayName("状态说明"), StringLength(128)]
        public string Description { get; set; } = "";

        internal virtual int GetDataBits()
        {
            return 1;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}