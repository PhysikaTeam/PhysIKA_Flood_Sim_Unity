

#ifndef VACUUM_WIREFRAME_COLORMASK0_CGINC
#define VACUUM_WIREFRAME_COLORMASK0_CGINC


//Curved World Compatibility
//#include "Assets/VacuumShaders/Curved World/Shaders/cginc/CurvedWorld_Base.cginc"
 

struct v2f   
{  
	float4 pos : SV_POSITION;	

	UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
};
	 
v2f vert(appdata_full v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f,o); 

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
//Curved World Compatibility
//V_CW_TransformPoint(v.vertex);

	o.pos = UnityObjectToClipPos(v.vertex);

	return o;
}

fixed4 frag () : SV_Target 
{
	return 0;
}

	
#endif
