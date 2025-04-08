using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class URN : MonoBehaviour
{
    public class Entity
    {
        public const int WORLD = 0x1101;
        public const int ROAD = 0x1102;
        public const int BLOCKADE = 0x1103;
        public const int BUILDING = 0x1104;
        public const int REFUGE = 0x1105;
        public const int HYDRANT = 0x1106;
        public const int GAS_STATION = 0x1107;
        public const int FIRE_STATION = 0x1108;
        public const int AMBULANCE_CENTRE = 0x1109;
        public const int POLICE_OFFICE = 0x110a;
        public const int CIVILIAN = 0x110b;
        public const int FIRE_BRIGADE = 0x110c;
        public const int AMBULANCE_TEAM = 0x110d;
        public const int POLICE_FORCE = 0x110e;
    }

    public class Property
    {
        public const int START_TIME = 0x1201;
        public const int LONGITUDE = 0x1202;
        public const int LATITUDE = 0x1203;
        public const int WIND_FORCE = 0x1204;
        public const int WIND_DIRECTION = 0x1205;
        public const int X = 0x1206;
        public const int Y = 0x1207;
        public const int BLOCKADES = 0x1208;
        public const int REPAIR_COST = 0x1209;
        public const int FLOORS = 0x120a;
        public const int BUILDING_ATTRIBUTES = 0x120b;
        public const int IGNITION = 0x120c;
        public const int FIERYNESS = 0x120d;
        public const int BROKENNESS = 0x120e;
        public const int BUILDING_CODE = 0x120f;
        public const int BUILDING_AREA_GROUND = 0x1210;
        public const int BUILDING_AREA_TOTAL = 0x1211;
        public const int APEXES = 0x1212;
        public const int EDGES = 0x1213;
        public const int POSITION = 0x1214;
        public const int DIRECTION = 0x1215;
        public const int POSITION_HISTORY = 0x1216;
        public const int STAMINA = 0x1217;
        public const int HP = 0x1218;
        public const int DAMAGE = 0x1219;
        public const int BURIEDNESS = 0x121a;
        public const int TRAVEL_DISTANCE = 0x121b;
        public const int WATER_QUANTITY = 0x121c;
        public const int TEMPERATURE = 0x121d;
        public const int IMPORTANCE = 0x121e;
        public const int CAPACITY = 0x121f;
        public const int BED_CAPACITY = 0x1220;
        public const int OCCUPIED_BEDS = 0x1221;
        public const int REFILL_CAPACITY = 0x1222;
        public const int WAITING_LIST_SIZE = 0x1223;
    }

    public class Command
    {
        public const int AK_REST = 0x1301;
        public const int AK_MOVE = 0x1302;
        public const int AK_LOAD = 0x1303;
        public const int AK_UNLOAD = 0x1304;
        public const int AK_SAY = 0x1305;
        public const int AK_TELL = 0x1306;
        public const int AK_EXTINGUISH = 0x1307;
        public const int AK_RESCUE = 0x1308;
        public const int AK_CLEAR = 0x1309;
        public const int AK_CLEAR_AREA = 0x130a;
        public const int AK_SUBSCRIBE = 0x130b;
        public const int AK_SPEAK = 0x130c;
    }

    public class ComponentCommand
    {
        public const int Target = 0x1401;
        public const int DestinationX = 0x1402;
        public const int DestinationY = 0x1403;
        public const int Water = 0x1404;
        public const int Path = 0x1405;
        public const int Message = 0x1406;
        public const int Channel = 0x1407;
        public const int Channels = 0x1408;
    }

    public class ControlMSG
    {
        public const int KG_CONNECT = 0x0101;
        public const int KG_ACKNOWLEDGE = 0x0102;
        public const int GK_CONNECT_OK = 0x0103;
        public const int GK_CONNECT_ERROR = 0x0104;
        public const int SK_CONNECT = 0x0105;
        public const int SK_ACKNOWLEDGE = 0x0106;
        public const int SK_UPDATE = 0x0107;
        public const int KS_CONNECT_OK = 0x0108;
        public const int KS_CONNECT_ERROR = 0x0109;
        public const int KS_UPDATE = 0x010a;
        public const int KS_COMMANDS = 0x010b;
        public const int KS_AFTERSHOCKS_INFO = 0x010c;
        public const int VK_CONNECT = 0x010d;
        public const int VK_ACKNOWLEDGE = 0x010e;
        public const int KV_CONNECT_OK = 0x010f;
        public const int KV_CONNECT_ERROR = 0x0110;
        public const int KV_TIMESTEP = 0x0111;
        public const int AK_CONNECT = 0x0112;
        public const int AK_ACKNOWLEDGE = 0x0113;
        public const int KA_CONNECT_OK = 0x0114;
        public const int KA_CONNECT_ERROR = 0x0115;
        public const int KA_SENSE = 0x0116;
        public const int SHUTDOWN = 0x0117;
        public const int ENTITY_ID_REQUEST = 0x0118;
        public const int ENTITY_ID_RESPONSE = 0x0119;
    }

    public class ComponentControlMSG
    {
        public const int RequestID = 0x0201;
        public const int AgentID = 0x0202;
        public const int Version = 0x0203;
        public const int Name = 0x0204;
        public const int RequestedEntityTypes = 0x0205;
        public const int SimulatorID = 0x0206;
        public const int RequestNumber = 0x0207;
        public const int NumberOfIDs = 0x0208;
        public const int NewEntityIDs = 0x0209;
        public const int Reason = 0x020a;
        public const int Entities = 0x020b;
        public const int ViewerID = 0x020c;
        public const int AgentConfig = 0x020d;
        public const int Time = 0x020e;
        public const int Updates = 0x020f;
        public const int Hearing = 0x0210;
        public const int INTENSITIES = 0x0211;
        public const int TIMES = 0x0212;
        public const int ID = 0x0213;
        public const int Commands = 0x0214;
        public const int SimulatorConfig = 0x0215;
        public const int Changes = 0x0216;
    }
}
