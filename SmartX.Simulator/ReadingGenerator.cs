using SmartX.Shared.Models;

//ST10445500 - PROG7312 - SmartX POE
//ReadingGenerator

//.....................................o0oSTART OF FILEo0o........................................//

// The generator produces the values one simulated device would be reporting.

namespace SmartX.Simulator
{
    //produces readings for one sensor that drift and occasionally spike
    //each sensor gets its own generator so its value carries on from where it was
    public class ReadingGenerator
    {
        //how often a reading is a deliberate spike, as one in this many
        private const int SpikeOdds = 25;

        //how quickly a reading outside its normal band is eased back toward the middle
        private const double RecoveryRate = 0.25;

        //the band a healthy moisture sensor sits in, and how far it moves per reading
        private const double MinMoisture = 35;
        private const double MaxMoisture = 60;
        private const double MoistureDrift = 1.5;

        // The gateway calls a moisture change a spike at fifteen percent, so a drop
        // has to clear that to be worth drawing on the ribbon.
        private const double MoistureSpikeDrop = 20;

        //the band a healthy power meter sits in, and how far it moves per reading
        private const double MinWatts = 200;
        private const double MaxWatts = 400;
        private const double WattDrift = 8;

        // Same idea as the moisture drop, against the gateway's fifty watt threshold.
        private const double WattSpikeJump = 90;

        private readonly SensorCategory _category;

        // Each generator holds its own Random rather than sharing one, because the
        // simulator runs a task per sensor and they would otherwise be drawing from
        // the same instance at the same time.
        private readonly Random _random;

        private double _value;
        private bool _state;

        public ReadingGenerator(SensorCategory category, int seed)
        {
            _category = category;
            _random = new Random(seed);

            _value = category == SensorCategory.PowerConsumption
                ? (MinWatts + MaxWatts) / 2
                : (MinMoisture + MaxMoisture) / 2;
        }

        //..............................................................................//

        //produces the next reading this sensor would report at the given time
        public DeviceReading Next(string macAddress, DateTime recordedAt)
        {
            var reading = new DeviceReading
            {
                MacAddress = macAddress,
                RecordedAt = recordedAt
            };

            switch (_category)
            {
                case SensorCategory.Environmental:
                    reading.Value = NextMoisture();
                    break;

                case SensorCategory.PowerConsumption:
                    reading.Value = NextWatts();
                    break;

                case SensorCategory.Actuator:
                    reading.State = NextState();
                    break;
            }

            return reading;
        }

        //..............................................................................//

        //produces the next soil moisture percentage
        private double NextMoisture()
        {
            if (IsSpike())
            {
                _value -= MoistureSpikeDrop;
            }
            else
            {
                _value += Wobble(MoistureDrift);
                _value = EaseBack(_value, MinMoisture, MaxMoisture);
            }

            // The gateway refuses a percentage outside nought to a hundred, so a run
            // of drops is not allowed to push it past the bottom.
            _value = Math.Clamp(_value, 0, 100);
            return Math.Round(_value, 1);
        }

        //..............................................................................//

        //produces the next wattage
        private double NextWatts()
        {
            if (IsSpike())
            {
                _value += WattSpikeJump;
            }
            else
            {
                _value += Wobble(WattDrift);
                _value = EaseBack(_value, MinWatts, MaxWatts);
            }

            _value = Math.Max(_value, 0);
            return Math.Round(_value);
        }

        //..............................................................................//

        //produces the next valve state, which mostly stays where it was
        private bool NextState()
        {
            // The gateway counts an actuator changing state as a spike, so flipping
            // it at the same odds as the other two keeps the ribbon consistent.
            if (IsSpike())
            {
                _state = !_state;
            }

            return _state;
        }

        //..............................................................................//

        //checks whether this reading should be a deliberate spike
        private bool IsSpike()
        {
            return _random.Next(SpikeOdds) == 0;
        }

        //..............................................................................//

        //produces a small random move either side of where the reading already was
        private double Wobble(double drift)
        {
            return ((_random.NextDouble() * 2) - 1) * drift;
        }

        //..............................................................................//

        //pulls a reading that has wandered outside its band back toward the middle of it
        private static double EaseBack(double value, double minimum, double maximum)
        {
            if (value >= minimum && value <= maximum)
            {
                return value;
            }

            // Without this a sensor that spiked would sit at its new value forever
            // and every later window would look normal again at the wrong level.
            var middle = (minimum + maximum) / 2;
            return value + ((middle - value) * RecoveryRate);
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
