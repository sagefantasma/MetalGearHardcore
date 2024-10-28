using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalGearHardcore
{
    internal class MGS3Stage
    {
        public enum LocationString
        {
            InvalidStage,
            //kyle_op, // Opening Snake Eater video also appears when Snake Easter video plays after the Virtuous Mission
            //title, // CQC Start screen and or Main Menu
            //theater, // Demo Theater Mode
            v001a, // Dremuchji South - Starting Area in Virtuous Mission
            s002a, // Dremuchji East - OSE Starting Area
            v003a, // Dremuchji Swampland - Croc area in V mission
            s003a, // Dremuchji Swampland - Croc area in V mission
            v004a, // Dremuchji North - First KGB Troop in V and where Boss breaks Snake's gun in SE
            s004a, // Dremuchji North - First KGB Troop in V and where Boss breaks Snake's gun in SE
            v005a, // Dolinovodno Rope Bridge - Rope Bridge
            s005a, // Dolinovodno Rope Bridge - Rope Bridge
            v006a, // Rassvet - Sokolov/Ocelot Unit
            v006b, // Rassvet - Sokolov/Ocelot Unit
            s006a, // Rassvet - Sokolov/Ocelot Unit
            s006b, // Rassvet - Sokolov/Ocelot Unit
            s007a, // VM Bridge Fall Aftermath Zone
            v007a, // Dolinovodno Riverbank - The Boss she betrayed me Major...
            s012a, // Chyornyj Prud - Swamp
            s021a, // Bolshaya Past South
            s022a, // Bolshaya Past North
            s023a, // Bolshaya Past Crevice - Ocelot Battle Area
            s031a, // Chyornaya Peschera Cave Branch - Cave After Ocelot
            s032a, // Chyornaya Peschera Cave - Before and After Pain boss fight
            s032b, // Chyornaya Peschera Cave - Pain Boss Battle
            s033a, // Chyornaya Peschera Cave Entrance
            s041a, // Ponizovje South - Water Area with the hovercrafts enemies
            s042a, // Ponizovje Armory - Where you get the SVD early
            s043a, // Ponizovje Warehouse Exterior - Outside where you can kill the end early
            s044a, // Ponizovje Warehouse - Interior of the warehouse
            s051a, // Graniny Gorki South - Fear Boss fight
            s051b, // Graniny Gorki South - Fear Boss fight
            s052a, // Graniny Gorki Lab Exterior: Outside Walls - Area after all traps/where you fight Fear later
            s052b, // Graniny Gorki Lab Exterior: Inside Walls 
            s053a, // Graniny Gorki Lab 1F/2F
            s054a, // Graniny Gorki Lab Interior
            s055a, // Graniny Gorki Lab B1 (Prison Cells)
            s056a, // Graniny Gorki Lab B1 (Granin Basement Area)
            s045a, // Svyatogornyj South - Just after Ponizovje Warehouse
            s061a, // Svyatogornyj West
            s062a, // Svyatogornyj East - M63 Area with the house
            s063a, // Sokrovenno South - The End Fight
            s063b, // Sokrovenno South - Ocelot Unit Fight 
            s064a, // Sokrovenno West - The End fight area with the river
            s064b, // Sokrovenno West - Ocelot Unit fight area with the river
            s065a, // Sokrovenno North - Area where The End dies and you head to the ladder area
            s065b, // Sokrovenno North - Final area of Sokrovenno next area is the ladder in Krasnogorje Tunnel
            s066a, // Krasnogorje Tunnel - What a thrill...
            s071a, // Krasnogorje Mountain Base - Start of the mountain area regardless of before/after Eva mountain cutscene
            s072a, // Krasnogorje Mountainside - a is hovercrafts 
            s072b, // Krasnogorje Mountainside - b is hind and for after Eva cutscene
            s073a, // Krasnogorje Mountaintop - Before cutscene with Eva
            s073b, // Krasnogorje Mountaintop - After cutscene with Eva
            s074a, // Krasnogorje Mountaintop Ruins - Eva Cutscene
            s075a, // Krasnogorje Mountaintop: Behind Ruins
            s081a, // Groznyj Grad Underground Tunnel - Before/During/After the fight with The Fury
            s091a, // Groznyj Grad Southwest - Area after fight with The Fury
            s091b, // Groznyj Grad Southwest - During Escape
            s091c, // Groznyj Grad Southwest - Back after escape
            s092a, // Groznyj Grad Northwest - Armory/ where you escape into the tunnels
            s092b, // Groznyj Grad Northwest - During Escape
            s092c, // Groznyj Grad Northwest - Back after escape
            s093a, // Groznyj Grad Northeast - Has the Enterance to the weapon's lab East Wing
            s093b, // Groznyj Grad Northeast - During Escape
            s093c, // Groznyj Grad Northeast - Back after escape
            s094a, // Groznyj Grad Southeast - Area with Enterance to the prison cells
            s094b, // Groznyj Grad Southeast - During Escape
            s094c, // Groznyj Grad Southeast - Back after escape
            s101a, // Groznyj Grad Weapon's Lab: East Wing - Where you take Raikov's outfit
            s101b, // Groznyj Grad Weapon's Lab: East Wing - After taking Raikov's outfit/Also when back to plant the C3
            s113a, // Groznyj Grad Sewers
            s112a, // Groznyj Grad Torture Room always this regardless of before, during, or after torture
            s111a, // Groznyj Grad Weapon's Lab: West Wing Corridor - Before you trigger Sokolov cutscene
            s121a, // Groznyj Grad Weapon's Lab: Main Wing
            s121b, // Groznyj Grad Weapon's Lab: Main Wing - C3 Mission
            s122a, // Groznyj Grad Weapon's Lab: Main Wing B1 - Volgin Fight both parts
            s141a, // Unsure on name of this area - Sorrow Boss Fight
            s151a, // Tikhogornyj - After Sorrow Fight regardless of if Ocelot Unit is there or not
            s152a, // Tikhogornyj - Behind Waterfall
            s161a, // Groznyj Grad - 1st Part of bike chase
            s162a, // Groznyj Grad Runway South - 2nd Part of bike chase
            s163a, // Groznyj Grad Runway - 3rd part of bike chase
            s163b, // Groznyj Grad Runway - 4th path of chase with shagohod only
            s171a, // Groznyj Grad Rail Bridge - Shooting the C3
            s171b, // Groznyj Grad Rail Bridge - Fighting the Shagohod on and off the bike
            s181a, // Groznyj Grad Rail Bridge North - 1st Escape after beating Volgin
            s182a, // Lazorevo South - Part of the chase with Hovercrafts
            s183a, // Lazorevo North - Final part of chase before on foot with Eva
            s191a, // Zaozyorje West - On Foot Area
            s192a, // Zaozyorje East - Final area before The Boss
            s201a, // Rokovj Bereg - Boss Arena
            //s211a, // Wig: Interior - When you pick which SAA
            //ending // Credits, etc
        }
    }
}
