using DraconiusGoGUI.Enums;
using System;
using System.Threading.Tasks;
using DraconiusGoGUI.Extensions;
using DracoLib.Core.Extensions;
using DracoProtos.Core.Base;
using DracoProtos.Core.Objects;
using DracoLib.Core.Exceptions;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        protected const double SpeedDownTo = 10 / 3.6;
        private double CurrentWalkingSpeed = 0;
        private Random WalkingRandom = new Random();

        private async Task<MethodResult> GoToLocation(GeoCoordinate location)
        {
            if (!UserSettings.MimicWalking)
            {
                MethodResult result = await UpdateLocation(location);

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenLocationUpdates, UserSettings.LocationupdateDelayRandom));

                return result;
            }

            const int maxTries = 3;
            int currentTries = 0;

            while (currentTries < maxTries)
            {
                try
                {
                    Func<Task<MethodResult>> walkingFunction = null;
                    Func<Task<MethodResult>> walkingIncenceFunction = null;

                    if (UserSettings.EncounterWhileWalking && UserSettings.CatchCreature)
                    {
                        walkingFunction = CatchNeabyCreature;
                        walkingIncenceFunction = CatchInsenceCreature;
                    }

                    MethodResult walkResponse = new MethodResult();
                    try
                    {
                        walkResponse = await WalkToLocation(location, walkingFunction, walkingIncenceFunction);
                        if (walkResponse.Success)
                        {
                            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenLocationUpdates, UserSettings.LocationupdateDelayRandom));

                            return new MethodResult
                            {
                                Success = true,
                                Message = "Successfully walked to location"
                            };
                        }
                    }
                    catch (Exception)
                    {
                        if (currentTries >= 3)
                        {
                            AccountState = AccountState.SoftBan;
                            LogCaller(new LoggerEventArgs(String.Format("Possible SoftBan, please wait one moment.."), LoggerTypes.Warning));
                            Stop();
                        }

                        LogCaller(new LoggerEventArgs(String.Format("Failed to walk to location. Retry #{0}", currentTries + 1), LoggerTypes.Warning));
                        _failedBuildingResponse++;
                    }
                    return new MethodResult();
                }
                catch (TaskCanceledException ex)
                {
                    throw ex;
                }
                catch (OperationCanceledException ex)
                {
                    throw ex;
                }
                finally
                {
                    ++currentTries;
                }
                //return new MethodResult();
            }

            return new MethodResult();
        }

        private double WalkOffset()
        {
            lock (_rand)
            {
                double maxOffset = UserSettings.WalkingSpeedOffset * 2;

                double offset = _rand.NextDouble() * maxOffset - UserSettings.WalkingSpeedOffset;

                return offset;
            }
        }

        private async Task<MethodResult> WalkToLocation(GeoCoordinate location, Func<Task<MethodResult>> functionExecutedWhileWalking, Func<Task<MethodResult>> functionExecutedWhileIncenseWalking)
        {
            double speedInMetersPerSecond = (UserSettings.WalkingSpeed + WalkOffset()) / 3.6;

            if (speedInMetersPerSecond <= 0)
            {
                speedInMetersPerSecond = 0;
            }

            if (CurrentWalkingSpeed == 0)
            {
                CurrentWalkingSpeed = VariantRandom(CurrentWalkingSpeed);
            }

            var destinaionCoordinate = new GeoCoordinate(location.Latitude, location.Longitude);
            var sourceLocation = new GeoCoordinate(UserSettings.Latitude, UserSettings.Longitude); ;
            var nextWaypointBearing = DegreeBearing(sourceLocation, destinaionCoordinate);
            var nextWaypointDistance = speedInMetersPerSecond;
            var waypoint = CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);
            var requestSendDateTime = DateTime.Now;
            var requestVariantDateTime = DateTime.Now;

            /*
            MethodResult _result = await UpdateLocation(waypoint);

            if (!_result.Success)
                return new MethodResult();
            */

            // Uneeded pause
            //await Task.Delay(CalculateDelay(UserSettings.DelayBetweenLocationUpdates, UserSettings.LocationupdateDelayRandom));

            do
            {
                var millisecondsUntilGetUpdatePlayerLocationResponse =
                    (DateTime.Now - requestSendDateTime).TotalMilliseconds;
                var millisecondsUntilVariant =
                    (DateTime.Now - requestVariantDateTime).TotalMilliseconds;

                sourceLocation = new GeoCoordinate(UserSettings.Latitude, UserSettings.Longitude); ;
                var currentDistanceToTarget = CalculateDistanceInMeters(sourceLocation, destinaionCoordinate);

                speedInMetersPerSecond = (UserSettings.WalkingSpeed + WalkOffset()) / 3.6;

                if (speedInMetersPerSecond <= 0)
                {
                    speedInMetersPerSecond = 0;
                }

                if (CurrentWalkingSpeed == 0)
                {
                    CurrentWalkingSpeed = VariantRandom(CurrentWalkingSpeed);
                }

                if (currentDistanceToTarget < 40)
                    if (speedInMetersPerSecond > SpeedDownTo)
                        speedInMetersPerSecond = SpeedDownTo;

                nextWaypointDistance = Math.Min(currentDistanceToTarget,
                    millisecondsUntilGetUpdatePlayerLocationResponse / 1000 * speedInMetersPerSecond);
                nextWaypointBearing = DegreeBearing(sourceLocation, destinaionCoordinate);
                waypoint = CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);

                requestSendDateTime = DateTime.Now;

                MethodResult result = await UpdateLocation(waypoint);

                // In this point we are sure that we are mimic walking but dont know if enable humazation is true
                var delay = CalculateDelay(UserSettings.DelayBetweenLocationUpdates, UserSettings.LocationupdateDelayRandom);
                
                // if enable humanization is false, then delay is 0 so we put a minimum delay of 10 seconds.
                delay = (delay == 0) ? 10000 + new Random().Next(0, 200): delay; 
                await Task.Delay(delay);

                if (!result.Success)
                    return new MethodResult();

                if (functionExecutedWhileWalking != null)
                    await functionExecutedWhileWalking(); // look for Creature

                if (functionExecutedWhileIncenseWalking != null)
                    await functionExecutedWhileIncenseWalking(); // look for incense Creature

            } while (CalculateDistanceInMeters(sourceLocation, destinaionCoordinate) >= (new Random()).Next(1, 10));

            return new MethodResult
            {
                Success = true,
                Message = "Success"
            };
        }

        private async Task<MethodResult> UpdateLocation(GeoCoordinate location)
        {
            try
            {
                var previousLocation = new GeoCoordinate(UserSettings.Latitude, UserSettings.Longitude);

                double distance = CalculateDistanceInMeters(previousLocation, location);

                //Prevent less than 1 meter hops
                if (distance < 1 && !_firstRun)
                {
                    return new MethodResult
                    {
                        Success = true
                    };
                }

                var moveTo = new GeoCoordinate(location.Latitude, location.Longitude);

                var realpos = await UpdateMap(moveTo.Latitude, moveTo.Longitude, (float)moveTo.HorizontalAccuracy);

                if (!realpos.Success)
                    throw new DracoError(realpos.Message);

                UserSettings.Latitude = moveTo.Latitude;
                UserSettings.Longitude = moveTo.Longitude;
                UserSettings.HorizontalAccuracy = moveTo.HorizontalAccuracy;

                string message = String.Format("Walked distance: {0:0.00}m", distance);

                LogCaller(new LoggerEventArgs(message, LoggerTypes.LocationUpdate));

                return new MethodResult
                {
                    Message = message,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to update location", LoggerTypes.Exception, ex));
                return new MethodResult();
            }
        }

        private bool IsValidLocation(double latitude, double longitude)
        {
            return latitude <= 90 && latitude >= -90 && longitude >= -180 && longitude <= 180;
        }

        private double CalculateDistanceInMeters(double sourceLat, double sourceLng,
                double destLat, double destLng)
        // from http://stackoverflow.com/questions/6366408/calculating-distance-between-two-latitude-and-longitude-geocoordinates
        {
            try
            {
                var sourceLocation = new GeoCoordinate(sourceLat, sourceLng);
                var targetLocation = new GeoCoordinate(destLat, destLng);
                return sourceLocation.GetDistanceTo(targetLocation);
            }
            catch (ArgumentOutOfRangeException)
            {
                return double.MaxValue;
            }
        }

        private double CalculateDistanceInMeters(GeoCoordinate sourceLocation, GeoCoordinate destinationLocation)
        {
            return CalculateDistanceInMeters(sourceLocation.Latitude, sourceLocation.Longitude,
                destinationLocation.Latitude, destinationLocation.Longitude);
        }

        private GeoCoordinate CreateWaypoint(GeoCoordinate sourceLocation,
                double distanceInMeters, double bearingDegrees)
        //from http://stackoverflow.com/a/17545955
        {
            var distanceKm = distanceInMeters / 1000.0;
            var distanceRadians = distanceKm / 6371; //6371 = Earth's radius in km

            var bearingRadians = ToRad(bearingDegrees);
            var sourceLatitudeRadians = ToRad(sourceLocation.Latitude);
            var sourceLongitudeRadians = ToRad(sourceLocation.Longitude);

            var targetLatitudeRadians = Math.Asin(Math.Sin(sourceLatitudeRadians) * Math.Cos(distanceRadians)
                                                  +
                                                  Math.Cos(sourceLatitudeRadians) * Math.Sin(distanceRadians) *
                                                  Math.Cos(bearingRadians));

            var targetLongitudeRadians = sourceLongitudeRadians + Math.Atan2(Math.Sin(bearingRadians)
                                                                             * Math.Sin(distanceRadians) *
                                                                             Math.Cos(sourceLatitudeRadians),
                                             Math.Cos(distanceRadians)
                                             - Math.Sin(sourceLatitudeRadians) * Math.Sin(targetLatitudeRadians));

            // adjust toLonRadians to be in the range -180 to +180...
            targetLongitudeRadians = (targetLongitudeRadians + 3 * Math.PI) % (2 * Math.PI) - Math.PI;

            return new GeoCoordinate(
                ToDegrees(targetLatitudeRadians),
                ToDegrees(targetLongitudeRadians)
            );
        }

        private GeoCoordinate CreateWaypoint(GeoCoordinate sourceLocation, double distanceInMeters,
                double bearingDegrees, double altitude)
        //from http://stackoverflow.com/a/17545955
        {
            var distanceKm = distanceInMeters / 1000.0;
            var distanceRadians = distanceKm / 6371; //6371 = Earth's radius in km

            var bearingRadians = ToRad(bearingDegrees);
            var sourceLatitudeRadians = ToRad(sourceLocation.Latitude);
            var sourceLongitudeRadians = ToRad(sourceLocation.Longitude);

            var targetLatitudeRadians = Math.Asin(Math.Sin(sourceLatitudeRadians) * Math.Cos(distanceRadians)
                                                  +
                                                  Math.Cos(sourceLatitudeRadians) * Math.Sin(distanceRadians) *
                                                  Math.Cos(bearingRadians));

            var targetLongitudeRadians = sourceLongitudeRadians + Math.Atan2(Math.Sin(bearingRadians)
                                                                             * Math.Sin(distanceRadians) *
                                                                             Math.Cos(sourceLatitudeRadians),
                                             Math.Cos(distanceRadians)
                                             - Math.Sin(sourceLatitudeRadians) * Math.Sin(targetLatitudeRadians));

            // adjust toLonRadians to be in the range -180 to +180...
            targetLongitudeRadians = (targetLongitudeRadians + 3 * Math.PI) % (2 * Math.PI) - Math.PI;

            return new GeoCoordinate(ToDegrees(targetLatitudeRadians), ToDegrees(targetLongitudeRadians), altitude);
        }

        private double DegreeBearing(GeoCoordinate sourceLocation, GeoCoordinate targetLocation)
        // from http://stackoverflow.com/questions/2042599/direction-between-2-latitude-longitude-points-in-c-sharp
        {
            var dLon = ToRad(targetLocation.Longitude - sourceLocation.Longitude);
            var dPhi = Math.Log(
                Math.Tan(ToRad(targetLocation.Latitude) / 2 + Math.PI / 4) /
                Math.Tan(ToRad(sourceLocation.Latitude) / 2 + Math.PI / 4));
            if (Math.Abs(dLon) > Math.PI)
                dLon = dLon > 0 ? -(2 * Math.PI - dLon) : 2 * Math.PI + dLon;
            return ToBearing(Math.Atan2(dLon, dPhi));
        }

        private double ToBearing(double radians)
        {
            // convert radians to degrees (as bearing: 0...360)
            return (ToDegrees(radians) + 360) % 360;
        }

        private double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        private double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        private double VariantRandom(double currentSpeed)
        {
            if (WalkingRandom.Next(1, 10) > 5)
            {
                var randomicSpeed = currentSpeed;
                var max = UserSettings.WalkingSpeed + UserSettings.WalkingSpeedOffset;
                randomicSpeed += WalkingRandom.NextDouble() * (0.02 - 0.001) + 0.001;

                if (randomicSpeed > max)
                    randomicSpeed = max;

                if (Math.Round(randomicSpeed, 2) != Math.Round(currentSpeed, 2))
                {
                    string message = String.Format("Current speed: {0:0.00}km/h. Randomized speed {1:0.00}km/h", currentSpeed, randomicSpeed);
                    LogCaller(new LoggerEventArgs(message, LoggerTypes.Success));
                    return randomicSpeed;
                }
            }
            else
            {
                var randomicSpeed = currentSpeed;
                var min = UserSettings.WalkingSpeed - UserSettings.WalkingSpeedOffset;
                randomicSpeed -= WalkingRandom.NextDouble() * (0.02 - 0.001) + 0.001;

                if (randomicSpeed < min)
                    randomicSpeed = min;

                if (Math.Round(randomicSpeed, 2) != Math.Round(currentSpeed, 2))
                {
                    string message = String.Format("Current speed: {0:0.00}km/h. Randomized speed {1:0.00}km/h", currentSpeed, randomicSpeed);
                    LogCaller(new LoggerEventArgs(message, LoggerTypes.Success));
                    return randomicSpeed;
                }
            }

            return currentSpeed;
        }       
    }
}