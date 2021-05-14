#pragma once
#include <assert.h>
#include <cuda_runtime.h>
#include "Core/Platform.h"
#include "MemoryManager.h"
namespace PhysIKA {

#define INVALID -1

	template<typename T, DeviceType deviceType = DeviceType::GPU>
	class Array2D
	{
	public:
		Array2D(const std::shared_ptr<MemoryManager<deviceType>> alloc = std::make_shared<DefaultMemoryManager<deviceType>>())
			: m_nx(0)
			, m_ny(0)
			, m_pitch(0)
			, m_totalNum(0)
			, m_data(NULL)
			, m_alloc(alloc)
		{};

		Array2D(int nx, int ny, const std::shared_ptr<MemoryManager<deviceType>> alloc = std::make_shared<DefaultMemoryManager<deviceType>>())
			: m_nx(nx)
			, m_ny(ny)
			, m_pitch(0)
			, m_totalNum(nx*ny)
			, m_data(NULL)
			, m_alloc(alloc)
		{
			AllocMemory();
		};

		/*!
		*	\brief	Should not release data here, call Release() explicitly.
		*/
		~Array2D() { };

		void resize(int nx, int ny);

		void Reset();

		void Release();

		inline T*		GetDataPtr() { return m_data; }
		void			SetDataPtr(T* _data) { m_data = _data; }

		COMM_FUNC inline int Nx() { return m_nx; }
		COMM_FUNC inline int Ny() { return m_ny; }

		COMM_FUNC inline T operator () (const int i, const int j) const
		{
			return m_data[i + j* m_pitch];
		}

		COMM_FUNC inline T& operator () (const int i, const int j)
		{
			return m_data[i + j* m_pitch];
		}

		COMM_FUNC inline int Index(const int i, const int j)
		{
			return i + j * m_pitch;
		}

		COMM_FUNC inline T operator [] (const int id) const
		{
			return m_data[id];
		}

		COMM_FUNC inline T& operator [] (const int id)
		{
			return m_data[id];
		}

		COMM_FUNC inline int Size() { return m_totalNum; }
		COMM_FUNC inline bool IsCPU() { return deviceType; }
		COMM_FUNC inline bool IsGPU() { return deviceType; }

	public:
		void AllocMemory();

	private:
		int m_nx;
		int m_ny;
		size_t m_pitch;
		int m_totalNum;
		T*	m_data;
		std::shared_ptr<MemoryManager<deviceType>> m_alloc;
	};

	template<typename T, DeviceType deviceType>
	void Array2D<T, deviceType>::resize(int nx, int ny)
	{
		if (NULL != m_data) Release();
		m_nx = nx;	m_ny = ny;	m_totalNum = m_nx*m_ny;
		AllocMemory();
	}

	template<typename T, DeviceType deviceType>
	void Array2D<T, deviceType>::Reset()
	{
		m_alloc->initMemory((void*)m_data, 0, m_pitch * m_ny * sizeof(T));
	}

	template<typename T, DeviceType deviceType>
	void Array2D<T, deviceType>::Release()
	{
		if (m_data != NULL)
		{
			m_alloc->releaseMemory((void**)&m_data);
		}

		m_data = NULL;
		m_nx = 0;
		m_ny = 0;
		m_pitch = 0;
		m_totalNum = 0;
	}

	template<typename T, DeviceType deviceType>
	void Array2D<T, deviceType>::AllocMemory()
	{
		m_alloc->allocMemory2D((void**)&m_data, m_pitch, m_nx, m_ny, sizeof(T));
		m_pitch /= sizeof(T);

		Reset();
	}

	template<typename T>
	using HostArray2D = Array2D<T, DeviceType::CPU>;

	template<typename T>
	using DeviceArray2D = Array2D<T, DeviceType::GPU>;
}
