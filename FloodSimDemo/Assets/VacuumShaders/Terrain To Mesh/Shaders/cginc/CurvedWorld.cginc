//Path to the CurvedWorld_Base.cginc file
//#include "Assets/VacuumShaders/Curved World/Shaders/cginc/CurvedWorld_Base.cginc"


//Defines for CurvedWorld enabled
// #define CURVED_WORLD_ENABLED
// #define CURVED_WORLD_TRANSFORM_POINT_AND_NORMAL(vertex,normal,tangent) V_CW_TransformPointAndNormal(vertex, normal, tangent);
// #define CURVED_WORLD_TRANSFORM_POINT(vertex)                           V_CW_TransformPoint(vertex);


//Defines for CurvedWorld disabled
#define CURVED_WORLD_DISABLED
#define CURVED_WORLD_TRANSFORM_POINT_AND_NORMAL(vertex,normal,tangent)
#define CURVED_WORLD_TRANSFORM_POINT(vertex)