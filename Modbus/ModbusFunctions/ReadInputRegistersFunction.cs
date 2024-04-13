using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read input registers functions/requests.
    /// </summary>
    public class ReadInputRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadInputRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadInputRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters modbus = (ModbusReadCommandParameters)CommandParameters;

            byte[] transactionIdBytes = BitConverter.GetBytes(modbus.TransactionId);
            byte[] protocolIdBytes = BitConverter.GetBytes(modbus.ProtocolId);
            byte[] lengthBytes = BitConverter.GetBytes(modbus.Length);
            byte[] startAddressBytes = BitConverter.GetBytes(modbus.StartAddress);
            byte[] quantityBytes = BitConverter.GetBytes(modbus.Quantity);

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
                startAddressBytes[1],  // Start Address high byte
                startAddressBytes[0],  // Start Address low byte
                quantityBytes[1],      // Quantity high byte
                quantityBytes[0]       // Quantity low byte
            };

            return ret;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> ret = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters modbus = (ModbusReadCommandParameters)CommandParameters;

            if (response.Length <= 9)
            {
                Console.WriteLine("Not valid message!");
            }
            else
            {
                for (int i = 0; i < response[8]; i += 2)
                {
                    Tuple<PointType, ushort> tuple = Tuple.Create(PointType.ANALOG_INPUT, modbus.StartAddress);
                    byte[] bytes = new byte[2];

                    bytes[0] = response[10 + i];
                    bytes[1] = response[9 + i];
                    ret.Add(tuple, (ushort)BitConverter.ToUInt16(bytes, 0));
                }
            }

            return ret;
        }
    }
}