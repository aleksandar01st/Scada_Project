using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters modbus = (ModbusWriteCommandParameters)CommandParameters;

            byte[] transactionIdBytes = BitConverter.GetBytes(modbus.TransactionId);
            byte[] protocolIdBytes = BitConverter.GetBytes(modbus.ProtocolId);
            byte[] lengthBytes = BitConverter.GetBytes(modbus.Length);
            byte[] outputAddressBytes = BitConverter.GetBytes(modbus.OutputAddress);
            byte[] valueBytes = BitConverter.GetBytes(modbus.Value);

            byte[] ret = new byte[12]
            {
                transactionIdBytes[1], // Transaction ID high byte
                transactionIdBytes[0], // Transaction ID low byte
                protocolIdBytes[1],    // Protocol ID high byte
                protocolIdBytes[0],    // Protocol ID low byte
                lengthBytes[1],        // Length high byte
                lengthBytes[0],        // Length low byte
                (byte)modbus.UnitId,   // Unit ID
                (byte)modbus.FunctionCode, // Function Code
                outputAddressBytes[1], // Output Address high byte
                outputAddressBytes[0], // Output Address low byte
                valueBytes[1],         // Value high byte
                valueBytes[0]          // Value low byte
            };

            return ret;
        }


        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> ret = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusWriteCommandParameters modbus = (ModbusWriteCommandParameters)CommandParameters;

            if (response.Length <= 9)
            {
                Console.WriteLine("Not valid message!");
            }
            else
            {
                for (int i = 0; i < response[8]; i += 2)
                {
                    Tuple<PointType, ushort> tuple = Tuple.Create(PointType.DIGITAL_OUTPUT, modbus.OutputAddress);
                    byte[] bytes = new byte[1];

                    bytes[0] = response[9 + i];
                    string bits = "";
                    foreach (byte b in bytes)
                    {
                        string bit = Convert.ToString(b, 2).PadLeft(8, '0');
                        bits += bit;
                    }
                    ret.Add(tuple, (ushort)Convert.ToUInt16(bits, 2));
                }
            }

            return ret;
        }
    }
}