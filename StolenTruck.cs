using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace CoolCallouts.Callouts
{
    [CalloutInfo("StolenTruck", CalloutProbibility.High)]
    public class StolenTruck : Callout
    {
        private Ped Suspect;
        private StolenTruck SuspectVehicle;
        private Vector3 SpawnPoint;
        private Blip SuspectBlip;
        private LHandle Pursuit;
        private bool PursuitCreated = false 

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(250f));

            // Shows the callout area
            ShowCalloutAreaBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(20f, SpawnPoint);

            // Shows the callout message
            CalloutMessage = "Stolen truck on";
            CalloutPosition = SpawnPoint;

            // Radio Audio when callout has been processed

            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GRAND_THEFT_AUTO IN_OR_ON_POSITION", SpawnPoint);

            return base.BeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            // Displays the suspects vehicle

            SuspectVehicle = new Vehicle("Flatbed", SpawnPoint);
            SuspectVehicle.IsPersistant = true;

            Suspect = SuspectVehicle.CreateRandomDriver();
            Suspect.IsPersistant = true;
            Suspect.BlockPermanentEvents = true;

            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.IsFriendly = false;

            Suspect.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.Emergency);
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {

            base.Process();
            if (!PursuitCreated && Game.LocalPlayer.Character.DistanceTo(Suspect.Position) <30) 
            {
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Suspect);
                Functions.SetPursuitActiveForPlayer(Pursuit, true);
                PursuitCreated = true;
            }

            if (PursuitCreated && !Functions.IsPursuitStillRunning(Pursuit))
            {

                End();
            }
        }

        public override void End()
        {

            base.End;
            if (Suspect.Exists()) {Suspect.Dismiss(); }
            if (SuspectVehicle.Exists()) { SuspectVehicle.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
        }
    }
}
