#pragma once
#include <iostream>
#include <chrono>
#include <algorithm>
#include <memory>
#include <cuda.h>
#include <cuda_runtime_api.h>
#include <GL/glew.h>
#include <GL/freeglut.h>



#include "GUI/GlutGUI/GLApp.h"

#include "Framework/Framework/SceneGraph.h"
#include "Framework/Framework/Log.h"

#include "Dynamics/RigidBody/RigidBody.h"
#include "Dynamics/HeightField/HeightFieldNode.h"

#include "Rendering/HeightFieldRender.h"

#include "IO\Image_IO\image.h"
#include "IO\Image_IO\png_io.h"
#include "IO\Image_IO\image_io.h"

using namespace std;
using namespace PhysIKA;

struct PhysIKAPointers {
	float* solid;
	float* depth;
	float* uVel;
	float* wVel;
};

extern "C"  __declspec (dllexport) void PhysIKAInit();

extern "C" __declspec (dllexport) int excuteOneStep(PhysIKAPointers* pData);
