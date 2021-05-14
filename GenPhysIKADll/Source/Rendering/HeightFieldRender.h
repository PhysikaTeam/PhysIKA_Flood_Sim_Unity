#pragma once

#include "Framework/Framework/ModuleVisual.h"
#include "Rendering/PointRender.h"
#include "Rendering/LineRender.h"
#include "Rendering/TriangleRender.h"
#include "Framework/Framework/FieldArray.h"
#include "Framework/Framework/FieldVar.h"

namespace PhysIKA
{
	class HeightFieldRenderModule : public VisualModule
	{
		DECLARE_CLASS(HeightFieldRenderModule)
	public:
		HeightFieldRenderModule();
		~HeightFieldRenderModule();

		enum RenderMode {
			POINT = 0,
			SPRITE,
			Instance
		};

		void display() override;
		void setRenderMode(RenderMode mode);
		void setColor(Vector3f color);

		void setColorRange(float min, float max);
		void setReferenceColor(float v);

	protected:
		bool  initializeImpl() override;

		void updateRenderingContext() override;

	private:
		RenderMode m_mode;
		Vector3f m_color;

		float m_refV;

		DeviceArray<float3> vertices;
		DeviceArray<float3> normals;
		DeviceArray<float3> colors;

		DeviceArray<glm::vec3> m_colorArray;

// 		std::shared_ptr<PointRenderUtil> point_render_util;
// 		std::shared_ptr<PointRenderTask> point_render_task;
		std::shared_ptr<PointRender> m_pointRender;
		std::shared_ptr<LineRender> m_lineRender;
		std::shared_ptr<TriangleRender> m_triangleRender;
	};

}