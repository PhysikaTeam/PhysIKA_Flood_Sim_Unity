#pragma once
#include <map>
#include <string>
typedef std::map<int, std::string> enum_map;
//typedef std::map<std::string, enum_map>	enum_map_list;

#define DECLARE_ENUM(enum_type,...)					\
enum enum_type{										\
	__VA_ARGS__										\
};													\
const std::string full_name_##enum_type = #__VA_ARGS__;

bool parse_enum_string(const std::string& enum_str, enum_map& enumKeyValueList);


class PEnum
{
public:
	PEnum(std::string enum_name, int val, const std::string enum_str)
	{
		parse_enum_string(enum_str, m_enum_map);
		m_enum_value = val;
		m_enum_name = enum_name;
	}
	
	inline bool operator== (const int val) const
	{
		return m_enum_value == val;
	}

	inline bool operator!= (const int val) const
	{
		return m_enum_value != val;
	}

	int currentKey() { return m_enum_value; }

private:
	std::string m_enum_name;
	int m_enum_value;

	enum_map m_enum_map;
};

#define DEF_ENUM(enum_name, enum_type, enum_value, desc)				\
private:									\
	VarField<PEnum> var_##enum_name = VarField<PEnum>(PEnum(#enum_type, enum_value, full_name_##enum_type), std::string(#enum_name), desc, FieldType::Param, this);			\
public:										\
	inline VarField<PEnum>* var##enum_name() {return &var_##enum_name;}


