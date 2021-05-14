#include "util.h"

HeightFieldNode<DataType3f> heightObj;
int cnt = 0;

extern "C"  __declspec (dllexport) void PhysIKAInit()
{
	std::string filename1 = "C:\\flood simulation\\pictures\\terrain4-4.png";//The pixel count is 1024*1024
	std::string filename2 = "C:\\flood simulation\\pictures\\river4-4.png";
	heightObj.loadParticlesFromImage(filename1, filename2, 100.0, 0.998);
}

extern "C" __declspec (dllexport) int excuteOneStep(PhysIKAPointers* pData)
{
	cnt++;
	float dt = 0.001;
	heightObj.run(1, dt);
	int size = 1024 * 1024 * sizeof(float);
	//auto start = std::chrono::steady_clock::now();
	heightObj.outputSolid();
	heightObj.outputDepth();
	heightObj.outputUVel();
	heightObj.outputWVel();
	pData->solid = heightObj.Solid.data();
	pData->depth = heightObj.Depth.data();
	pData->uVel = heightObj.UVel.data();
	pData->wVel = heightObj.WVel.data();
	/*auto end = std::chrono::steady_clock::now();
	double ret = std::chrono::duration<double>(end - start).count();*/
	return cnt;
}