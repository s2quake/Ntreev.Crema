//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#include "crema_reader.h"
#include <stdarg.h>
#include <locale>
#include <sstream>
#include <iostream>
#include <fstream>

namespace CremaCode {
	namespace reader {
		using namespace internal;
		class enum_data
		{
		public:
			static const enum_info* get_enum_info(const std::string& name)
			{
				return (*enum_data::enum_infos.find(name)).second;
			}

		public:
			static std::map<std::string, const enum_info*> enum_infos;
		};

		std::map<std::string, const enum_info*> enum_data::enum_infos;

		enum_info::enum_info(bool isflag)
			: m_isflag(isflag)
		{
			m_values = new std::map<std::string, int>();
			m_names = new std::map<int, std::string>();
		}

		enum_info::~enum_info()
		{
			delete m_values;
			delete m_names;
		}

		void enum_info::add(const std::string& name, int value)
		{
			m_values->insert(std::map<std::string, int>::value_type(name, value));
			m_names->insert(std::map<int, std::string>::value_type(value, name));
		}

		int enum_info::value(const std::string& name) const
		{
			return (*m_values->find(name)).second;
		}

		std::string enum_info::name(int value) const
		{
			std::map<int, std::string>::const_iterator itor = m_names->find(value);
			if (itor == m_names->end())
				return string_resource::empty_string;
			return itor->second;
		}

		std::vector<std::string> enum_info::names() const
		{
			std::vector<std::string> n;
			n.reserve(m_values->size());

			for (std::map<std::string, int>::const_iterator itor = m_values->begin(); itor != m_values->end(); itor++)
			{
				n.push_back(itor->first);
			}

			return n;
		}

		int enum_info::parse(const std::string& text) const
		{
			if (m_isflag == false)
				return this->value(text);

			int value = 0;
			std::istringstream iss(text);
			std::string buffer;
			while (std::getline(iss, buffer, ' '))
			{
				value |= this->value(buffer);
			}
			return value;
		}

		std::string enum_info::to_string(int value) const
		{
			std::map<int, std::string>::const_iterator itorFind = m_names->find(value);

			if (itorFind != m_names->end())
				return itorFind->second;

			std::stringstream ss;

			if (m_isflag == false)
			{
				ss << value;
				return ss.str();
			}

			int value2 = 0;
			std::vector<std::string> items;
			items.reserve(m_names->size());
			for (std::map<int, std::string>::iterator itor = m_names->begin(); itor != m_names->end(); itor++)
			{
				int mask = itor->first;
				if (value != 0 && mask == 0)
					continue;

				if ((value & mask) == mask)
				{
					items.push_back(itor->second);
					value2 |= itor->first;
				}
			}

			if (value2 == value)
			{
				for (size_t i = 0; i < items.size(); i++)
				{
					if (i != 0)
					{
						ss << " ";
					}
					ss << items[i];
				}
			}
			else
			{
				ss << value;
			}

			return ss.str();
		}

		bool enum_info::is_flag() const
		{
			return m_isflag;
		}

		int enum_util::parse(const std::type_info& typeinfo, const std::string& text)
		{
			const enum_info* enum_info = enum_data::get_enum_info(typeinfo.name());
			if (enum_info == nullptr)
				throw std::invalid_argument(string_resource::invalid_type);
			return enum_info->parse(text);
		}

		void enum_util::add(const std::type_info& typeinfo, const enum_info* penum_info)
		{
			enum_data::enum_infos.insert(std::make_pair(typeinfo.name(), penum_info));
		}

		bool enum_util::contains(const std::type_info& typeinfo)
		{
			return enum_data::enum_infos.find(typeinfo.name()) != enum_data::enum_infos.end();
		}

		std::string enum_util::name(const std::type_info& typeinfo, int value)
		{
			const enum_info* enum_info = enum_data::get_enum_info(typeinfo.name());
			if (enum_info == nullptr)
				throw std::invalid_argument(string_resource::invalid_type);
			return enum_info->name(value);
		}

		std::vector<std::string> enum_util::names(const std::type_info& typeinfo)
		{
			const enum_info* enum_info = enum_data::get_enum_info(typeinfo.name());
			if (enum_info == nullptr)
				throw std::invalid_argument(string_resource::invalid_type);
			return enum_info->names();
		}

		std::string enum_util::to_string(const std::type_info& typeinfo, int value)
		{
			const enum_info* enum_info = enum_data::get_enum_info(typeinfo.name());
			if (enum_info == nullptr)
				throw std::invalid_argument(string_resource::invalid_type);
			return enum_info->to_string(value);
		}

		bool enum_util::is_flag(const std::type_info& typeinfo)
		{
			const enum_info* enum_info = enum_data::get_enum_info(typeinfo.name());
			if (enum_info == nullptr)
				throw std::invalid_argument(string_resource::invalid_type);
			return enum_info->is_flag();
		}

		short convert::to_int16(const std::string& text)
		{
			return (short)to_int32(text);
		}

		int convert::to_int32(const std::string& text)
		{
			return atoi(text.c_str());
		}

		long long convert::to_int64(const std::string& text)
		{
#ifdef _MSC_VER
			return _atoi64(text.c_str());
#else
			return atoll(text.c_str());
#endif
		}

		unsigned int convert::to_uint32(const std::string& text)
		{
			return atoi(text.c_str());
		}

		float convert::to_float(const std::string& text)
		{
			return (float)atof(text.c_str());
		}

		bool convert::to_boolean(const std::string& text)
		{
			if (text == "true")
				return true;
			return false;
		}

		//#ifndef _IGNORE_BOOST
		//		std::string iniutil::utf8_to_string(const char* text)
		//		{
		//			return boost::locale::conv::from_utf<char>(text, "euc-kr");
		//		}
		//#else
		std::string iniutil::utf8_to_string(const char* text)
		{
			return text;
		}
		//#endif

		const std::type_info& iniutil::name_to_type(const std::string& typeName)
		{
			if (typeName.compare("boolean") == 0)
				return typeid(bool);
			else if (typeName.compare("float") == 0)
				return typeid(float);
			else if (typeName.compare("double") == 0)
				return typeid(double);
			else if (typeName.compare("int8") == 0)
				return typeid(char);
			else if (typeName.compare("uint8") == 0)
				return typeid(unsigned char);
			else if (typeName.compare("int16") == 0)
				return typeid(short);
			else if (typeName.compare("uint16") == 0)
				return typeid(unsigned short);
			else if (typeName.compare("int32") == 0)
				return typeid(int);
			else if (typeName.compare("uint32") == 0)
				return typeid(unsigned int);
			else if (typeName.compare("int64") == 0)
				return typeid(long long);
			else if (typeName.compare("uing64") == 0)
				return typeid(unsigned long long);
			else if (typeName.compare("datetime") == 0)
				return typeid(long long);
			else if (typeName.compare("duration") == 0)
				return typeid(long long);
			else if (typeName.compare("guid") == 0)
				return typeid(std::string);
			else if (typeName.compare("string") == 0)
				return typeid(std::string);

			return typeid(int);
		}

		std::wstring iniutil::string_to_wstring(const std::string& text)
		{
			if (text.length() == 0)
				return L"";
			setlocale(LC_ALL, "");
			size_t len;
#ifdef _MSC_VER
			mbstowcs_s(&len, NULL, NULL, text.c_str(), text.length());
			std::vector<wchar_t> buffer(len + 1);
			buffer[len] = 0;
			mbstowcs_s(&len, &buffer.front(), len, text.c_str(), text.length());
#else
			len = mbstowcs(NULL, text.c_str(), text.length());
			std::vector<wchar_t> buffer(len + 1);
			buffer[len] = 0;
			mbstowcs(&buffer.front(), text.c_str(), text.length());
#endif


			std::wstring retVal(&buffer.front());
			return retVal;
		}

		std::string iniutil::to_lower(const std::string& text)
		{
			std::string data = text;
			std::transform(data.begin(), data.end(), data.begin(), ::tolower);
			return data;
		}

		//unsigned long hash(const std::string& str)
		//{
		//	unsigned long hash = 5381;
		//	for (size_t i = 0; i < str.size(); ++i)
		//		hash = 33 * hash + (unsigned char)str[i];
		//	return hash;
		//}

		int iniutil::get_hash_code(const std::string& text)
		{
			std::wstring wtext = string_to_wstring(text);
			const wchar_t* chPtr = wtext.c_str();

			int num = 0x15051505;
			int num2 = num;
			int* numPtr = (int*)chPtr;
			for (int i = (int)wtext.length(); i > 0; i -= 4)
			{
				num = (((num << 5) + num) + (num >> 0x1b)) ^ numPtr[0];
				if (i <= 2)
					break;
				num2 = (((num2 << 5) + num2) + (num2 >> 0x1b)) ^ numPtr[1];
				numPtr += 2;
			}
			return num + (num2 * 0x5d588b65);
		}

		long iniutil::generate_hash_core(size_t count, size_t keysize ...)
		{
			va_list vl;

			va_start(vl, keysize);
			size_t offset = 0;
			std::vector<char> fields(keysize);
			const std::collate<char>& coll = std::use_facet< std::collate<char> >(std::locale());

			for (size_t i = 0; i < count; i++)
			{
				const std::type_info& typeinfo = *va_arg(vl, const std::type_info*);
#ifdef _DEBUG
				const char* name = typeinfo.name();
#endif

				if (typeinfo == typeid(bool))
				{
					internal_util::set_field_value(&fields.front(), offset, !!va_arg(vl, int));
				}
				else if (typeinfo == typeid(char))
				{
					internal_util::set_field_value(&fields.front(), offset, (char)va_arg(vl, int));
				}
				else if (typeinfo == typeid(unsigned char))
				{
					internal_util::set_field_value(&fields.front(), offset, (unsigned char)va_arg(vl, int));
				}
				else if (typeinfo == typeid(short))
				{
					internal_util::set_field_value(&fields.front(), offset, (short)va_arg(vl, int));
				}
				else if (typeinfo == typeid(unsigned short))
				{
					internal_util::set_field_value(&fields.front(), offset, (unsigned short)va_arg(vl, int));
				}
				else if (typeinfo == typeid(int))
				{
					internal_util::set_field_value(&fields.front(), offset, (int)va_arg(vl, int));
				}
				else if (typeinfo == typeid(unsigned int))
				{
					internal_util::set_field_value(&fields.front(), offset, (unsigned int)va_arg(vl, int));
				}
				else if (typeinfo == typeid(float))
				{
					internal_util::set_field_value(&fields.front(), offset, (float)va_arg(vl, double));
				}
				else if (typeinfo == typeid(double))
				{
					internal_util::set_field_value(&fields.front(), offset, (double)va_arg(vl, double));
				}
				else if (typeinfo == typeid(long long))
				{
					internal_util::set_field_value(&fields.front(), offset, (long long)va_arg(vl, long long));
				}
				else if (typeinfo == typeid(unsigned long long))
				{
					internal_util::set_field_value(&fields.front(), offset, (unsigned long long)va_arg(vl, long long));
				}
				else if (typeinfo == typeid(char*) || typeinfo == typeid(const char*))
				{
					int stringID = iniutil::get_hash_code(va_arg(vl, const char*));
					internal_util::set_field_value(&fields.front(), offset, stringID);
				}
				else if (typeinfo == typeid(std::string))
				{
					const std::string& text = *(const std::string*)va_arg(vl, const char*);
					int stringID = iniutil::get_hash_code(text);
					internal_util::set_field_value(&fields.front(), offset, stringID);
				}
				else
				{
					internal_util::set_field_value(&fields.front(), offset, (int)va_arg(vl, int));
				}
			}
			va_end(vl);

			long hash = coll.hash(&fields.front(), &fields.front() + fields.size());

			return hash;
		}

		size_t iniutil::keys_size(size_t count, ...)
		{
			va_list vl;
			va_start(vl, count);
			size_t size = 0;

			for (size_t i = 0; i < count; i++)
			{
				const std::type_info& typeinfo = *va_arg(vl, const std::type_info*);
				if (typeinfo == typeid(bool))
				{
					size += sizeof(int);
				}
				else if (typeinfo == typeid(char))
				{
					size += sizeof(int);
				}
				else if (typeinfo == typeid(unsigned char))
				{
					size += sizeof(int);
				}
				else if (typeinfo == typeid(short))
				{
					size += sizeof(int);
				}
				else if (typeinfo == typeid(unsigned short))
				{
					size += sizeof(int);
				}
				else if (typeinfo == typeid(int))
				{
					size += sizeof(int);
				}
				else if (typeinfo == typeid(unsigned int))
				{
					size += sizeof(int);
				}
				else if (typeinfo == typeid(float))
				{
					size += sizeof(float);
				}
				else if (typeinfo == typeid(double))
				{
					size += sizeof(double);
				}
				else if (typeinfo == typeid(long long))
				{
					size += sizeof(long long);
				}
				else if (typeinfo == typeid(unsigned long long))
				{
					size += sizeof(long long);
				}
				else if (typeinfo == typeid(std::string))
				{
					size += sizeof(int);
				}
				else
				{
					size += sizeof(int);
				}
			}
			return size;
		}

		const int s_magic_value = 0x04000000;

		std::map<int, std::string> string_resource::m_strings;
		std::string string_resource::empty_string;
		int string_resource::m_ref = 0;
		internal_util static_data;

		CremaReader::CremaReader()
			: m_stream(nullptr)
		{
			string_resource::add_ref();
			static_data.m_readers.push_back(this);
		}

		CremaReader::~CremaReader()
		{
			static_data.m_readers.remove(this);
			string_resource::remove_ref();
			if (m_stream != nullptr)
			{
				delete m_stream;
				m_stream = nullptr;
			}
		}

		//#ifndef _IGNORE_BOOST
		//		CremaReader& CremaReader::read(const std::string& address, int port, const std::string& name, ReadFlag flag)
		//		{
		//			socket_istream* stream = new socket_istream(address, port + 1, name);
		//			CremaReader& reader = CremaReader::read(*stream, flag);
		//			reader.m_stream = stream;
		//			return reader;
		//		}
		//
		//		CremaReader& CremaReader::read(const std::string& address, int port, const std::string& database, DataLocation datalocation, ReadFlag flag)
		//		{
		//			std::string dl;
		//			switch (datalocation)
		//			{
		//			case DataLocation_both:
		//				dl = "Both";
		//				break;
		//			case DataLocation_server:
		//				dl = "Server";
		//				break;
		//			case DataLocation_client:
		//				dl = "Client";
		//				break;
		//			default:
		//				break;
		//			}
		//			std::ostringstream ss;
		//			ss << "type=bin;data=" << dl << ";database=" << database << ";";
		//			return read(address, port, ss.str(), flag);
		//		}
		//#endif

		CremaReader& CremaReader::read(std::istream& stream, ReadFlag flag)
		{
			std::ifstream* fstream = dynamic_cast<std::ifstream*>(&stream);
			if (fstream != nullptr && fstream->is_open() == false)
				throw std::invalid_argument("파일이 열리지 않았습니다.");

			unsigned int magicValue;
			stream.read((char*)&magicValue, sizeof(int));

			if (magicValue == s_magic_value)
			{
				binary::binary_reader* reader = new binary::binary_reader();
				reader->read_core(stream, flag);
				return *reader;
			}

			throw std::exception();

			//libxml2_reader* reader = new libxml2_reader();
			//reader->read_core(fstream, flag);
			//return *reader;

		}

		CremaReader& CremaReader::read(const std::string& filename, ReadFlag flag)
		{
#ifdef _MSC_VER
			std::ifstream* stream = new std::ifstream(filename, std::ios::binary);
#else
			std::ifstream* stream = new std::ifstream(filename.c_str(), std::ios::binary);
#endif
			CremaReader& reader = CremaReader::read(*stream, flag);
			reader.m_stream = stream;
			return reader;
		}

#ifdef _MSC_VER
		keynotfoundexception::keynotfoundexception(const std::string& key, const std::string& container_name)
			: out_of_range(format_string(key, container_name).c_str())
		{

		}
#else
		keynotfoundexception::keynotfoundexception(const std::string& key, const std::string& container_name)
			: out_of_range(format_string(key, container_name).c_str())
		{

	}
#endif

		std::string keynotfoundexception::format_string(const std::string& key, const std::string& container_name)
		{
			std::stringstream text;
			text << container_name << "에 " << key << "라는 항목은 존재하지 않습니다.";
			return text.str();
		}

		inicolumn& inikey_array::operator [] (size_t index) const
		{
			return this->at(index);
		}

		inicolumn& icolumn_array::operator [] (const std::string& columnName) const
		{
			return this->at(columnName);
		}

		inicolumn& icolumn_array::operator [] (size_t index) const
		{
			return this->at(index);
		}

		irow& irow_array::operator [] (size_t index) const
		{
			return this->at(index);
		}

		itable& itable_array::operator [] (const std::string& tableName) const
		{
			return this->at(tableName);
		}

		itable& itable_array::operator [] (size_t index) const
		{
			return this->at(index);
		}

		bool irow::has_value(const std::string& columnName) const
		{
			return this->has_value_core(this->table().columns().at(columnName));
		}

		bool irow::has_value(size_t index) const
		{
			return this->has_value_core(this->table().columns().at(index));
		}

		bool irow::has_value(const inicolumn& column) const
		{
			return this->has_value_core(column);
		}

		bool irow::to_boolean(const std::string& columnName) const
		{
			return this->value<bool>(this->table().columns().at(columnName));
		}

		bool irow::to_boolean(size_t index) const
		{
			return this->value<bool>(this->table().columns().at(index));
		}

		bool irow::to_boolean(const inicolumn& column) const
		{
			return this->value<bool>(column);
		}

		const std::string& irow::to_string(const std::string& columnName) const
		{
			return this->value<std::string>(this->table().columns().at(columnName));
		}

		const std::string& irow::to_string(size_t index) const
		{
			return this->value<std::string>(this->table().columns().at(index));
		}

		const std::string& irow::to_string(const inicolumn& column) const
		{
			return this->value<std::string>(column);
		}

		float irow::to_single(const std::string& columnName) const
		{
			return this->value<float>(this->table().columns().at(columnName));
		}

		float irow::to_single(size_t index) const
		{
			return this->value<float>(this->table().columns().at(index));
		}

		float irow::to_single(const inicolumn& column) const
		{
			return this->value<float>(column);
		}

		double irow::to_double(const std::string& columnName) const
		{
			return this->value<double>(this->table().columns().at(columnName));
		}

		double irow::to_double(size_t index) const
		{
			return this->value<double>(this->table().columns().at(index));
		}

		double irow::to_double(const inicolumn& column) const
		{
			return this->value<double>(column);
		}

		char irow::to_int8(const std::string& columnName) const
		{
			return this->value<char>(this->table().columns().at(columnName));
		}

		char irow::to_int8(size_t index) const
		{
			return this->value<char>(this->table().columns().at(index));
		}

		char irow::to_int8(const inicolumn& column) const
		{
			return this->value<char>(column);
		}

		unsigned char irow::to_uint8(const std::string& columnName) const
		{
			return this->value<unsigned char>(this->table().columns().at(columnName));
		}

		unsigned char irow::to_uint8(size_t index) const
		{
			return this->value<unsigned char>(this->table().columns().at(index));
		}

		unsigned char irow::to_uint8(const inicolumn& column) const
		{
			return this->value<unsigned char>(column);
		}

		short irow::to_int16(const std::string& columnName) const
		{
			return this->value<short>(this->table().columns().at(columnName));
		}

		short irow::to_int16(size_t index) const
		{
			return this->value<short>(this->table().columns().at(index));
		}

		short irow::to_int16(const inicolumn& column) const
		{
			return this->value<short>(column);
		}

		unsigned short irow::to_uint16(const std::string& columnName) const
		{
			return this->value<unsigned short>(this->table().columns().at(columnName));
		}

		unsigned short irow::to_uint16(size_t index) const
		{
			return this->value<unsigned short>(this->table().columns().at(index));
		}

		unsigned short irow::to_uint16(const inicolumn& column) const
		{
			return this->value<unsigned short>(column);
		}

		int irow::to_int32(const std::string& columnName) const
		{
			return this->value<int>(this->table().columns().at(columnName));
		}

		int irow::to_int32(size_t index) const
		{
			return this->value<int>(this->table().columns().at(index));
		}

		int irow::to_int32(const inicolumn& column) const
		{
			return this->value<int>(column);
		}

		unsigned int irow::to_uint32(const std::string& columnName) const
		{
			return this->value<unsigned int>(this->table().columns().at(columnName));
		}

		unsigned int irow::to_uint32(size_t index) const
		{
			return this->value<unsigned int>(this->table().columns().at(index));
		}

		unsigned int irow::to_uint32(const inicolumn& column) const
		{
			return this->value<unsigned int>(column);
		}

		long long irow::to_int64(const std::string& columnName) const
		{
			return this->value<long long>(this->table().columns().at(columnName));
		}

		long long irow::to_int64(size_t index) const
		{
			return this->value<long long>(this->table().columns().at(index));
		}

		long long irow::to_int64(const inicolumn& column) const
		{
			return this->value<long long>(column);
		}

		unsigned long long irow::to_uint64(const std::string& columnName) const
		{
			return this->value<unsigned long long>(this->table().columns().at(columnName));
		}

		unsigned long long irow::to_uint64(size_t index) const
		{
			return this->value<unsigned long long>(this->table().columns().at(index));
		}

		unsigned long long irow::to_uint64(const inicolumn& column) const
		{
			return this->value<unsigned long long>(column);
		}


		time_t irow::to_datetime(const std::string& columnName) const
		{
			return (time_t)this->value<long long>(columnName);
		}

		time_t irow::to_datetime(size_t index) const
		{
			return (time_t)this->value<long long>(index);
		}

		time_t irow::to_datetime(const inicolumn& column) const
		{
			return (time_t)this->value<long long>(column);
		}

		long long irow::to_duration(const std::string& columnName) const
		{
			return this->value<long long>(columnName);
		}

		long long irow::to_duration(size_t index) const
		{
			return this->value<long long>(index);
		}

		long long irow::to_duration(const inicolumn& column) const
		{
			return this->value<long long>(column);
		}

		namespace internal {
			std::string string_resource::invalid_type("잘못된 타입입니다.");

			std::string empty_string = "";

			internal_util::internal_util()
			{

			}

			internal_util::~internal_util()
			{
				std::list<CremaReader*> readers;
				readers.assign(m_readers.begin(), m_readers.end());

				for (std::list<CremaReader*>::iterator itor = readers.begin(); itor != readers.end(); itor++)
				{
					(*itor)->destroy();
				}
			}

			void string_resource::read(std::istream& stream)
			{
				int stringCount;
				stream.read((char*)&stringCount, sizeof(int));
				for (int i = 0; i < stringCount; i++)
				{
					int length, id;
					stream.read((char*)&id, sizeof(int));
					stream.read((char*)&length, sizeof(int));

					if (m_strings.find(id) == m_strings.end())
					{
						std::string text;
						if (length != 0)
						{
							std::vector<char> buffer(length + 1, 0);
							stream.read(&buffer.front(), length);
							text = iniutil::utf8_to_string(&buffer.front());
						}

						m_strings[id] = text;
					}
					else
					{
#ifdef _DEBUG
						std::streamoff off = stream.tellg();
#endif
						stream.seekg(length, std::ios::cur);
					}
				}
			}

			const std::string& string_resource::get(int id)
			{
				if (id == 0)
					return empty_string;
				std::map<int, std::string>::const_iterator itor = m_strings.find(id);
				return itor->second;
			}

			void string_resource::add_ref()
			{
				m_ref++;
			}

			void string_resource::remove_ref()
			{
				m_ref--;

				if (m_ref == 0)
					m_strings.clear();
			}

			namespace binary
			{
				binary_reader::binary_reader()
					: m_tables(*this)
				{

				}

				binary_reader::~binary_reader()
				{

				}

				void binary_reader::destroy()
				{
					delete this;
				}

				void binary_reader::read_core(std::istream& stream, ReadFlag flag)
				{
					file_header fileHeader;
					m_stream = &stream;
					m_flag = flag;

					stream.seekg(0, std::ios_base::beg);
					stream.read((char*)&fileHeader, sizeof(file_header));
					m_tableIndexes.assign(fileHeader.tableCount, table_index());
					if (fileHeader.tableCount == 0)
						return;
					stream.read((char*)&m_tableIndexes.front(), sizeof(table_index) * fileHeader.tableCount);
					stream.seekg(fileHeader.stringResourcesOffset);
					string_resource::read(stream);
					m_name = string_resource::get(fileHeader.name);
					m_revision = string_resource::get(fileHeader.revision);

					this->m_tables.set_size(m_tableIndexes);
					this->m_tables.set_flag(flag);
					m_typesHashValue = string_resource::get(fileHeader.typesHashValue);
					m_tablesHashValue = string_resource::get(fileHeader.tablesHashValue);
					m_tags = string_resource::get(fileHeader.tags);

					if ((flag & ReadFlag_lazy_loading) == false)
					{
						for (size_t i = 0; i < m_tableIndexes.size(); i++)
						{
							const table_index& tableIndex = m_tableIndexes[i];

							binary_table* table = read_table(stream, tableIndex.offset, flag);
							this->m_tables.set(i, table);
						}
					}
				}

				binary_table* binary_reader::read_table(size_t index)
				{
					const table_index& table_index = m_tableIndexes.at(index);
					binary_table* table = binary_reader::read_table(*m_stream, table_index.offset, m_flag);
					this->m_tables.set(index, table);
					return table;
				}

				bool binary_reader::findif::operator() (const table_index& index)
				{
					if ((m_flag & ReadFlag_case_sensitive) != 0)
						return m_tableName == string_resource::get(index.tableName);
					return m_tableName == iniutil::to_lower(string_resource::get(index.tableName));
				}

				binary_table* binary_reader::read_table(const std::string& tableName)
				{
					std::vector<table_index>::const_iterator itor = std::find_if(m_tableIndexes.begin(), m_tableIndexes.end(), findif(m_flag, tableName));

					if (itor == m_tableIndexes.end())
						throw keynotfoundexception(tableName, "tables");

					size_t index = itor - m_tableIndexes.begin();

					binary_table* table = binary_reader::read_table(*m_stream, itor->offset, m_flag);
					this->m_tables.set(index, table);
					return table;
				}

				binary_table* binary_reader::read_table(std::istream& stream, std::streamoff offset, ReadFlag flag)
				{
					table_header tableHeader;
					table_info tableInfo;

					stream.seekg(offset, std::ios::beg);
					stream.read((char*)&tableHeader, sizeof(table_header));

					stream.seekg(tableHeader.tableInfoOffset + offset, std::ios::beg);
					stream.read((char*)&tableInfo, sizeof(table_info));

					binary_table* table = new binary_table(this, tableInfo.columnCount, tableInfo.rowCount);

					stream.seekg(tableHeader.stringResourcesOffset + offset, std::ios::beg);
					string_resource::read(stream);

					stream.seekg(tableHeader.columnsOffset + offset);
					binary_reader::read_columns(stream, *table, tableInfo.columnCount, flag);

					stream.seekg(tableHeader.rowsOffset + offset, std::ios::beg);
					binary_reader::read_rows(stream, *table, tableInfo.rowCount);

					table->m_tableName = string_resource::get(tableInfo.tableName);
					table->m_categoryName = string_resource::get(tableInfo.categoryName);
					table->m_hashValue = string_resource::get(tableHeader.hashValue);
					return table;
				}

				void binary_reader::read_columns(std::istream& stream, binary_table& table, size_t columnCount, ReadFlag flag)
				{
					table.m_columns.set_flag(flag);

					for (size_t i = 0; i < columnCount; i++)
					{
						column_info columninfo;
						stream.read((char*)&columninfo, sizeof(column_info));

						const std::string& columnName = string_resource::get(columninfo.columnName);
						const std::string& typeName = string_resource::get(columninfo.dataType);
						bool isKey = columninfo.iskey == 0 ? false : true;

						binary_column* column = new binary_column(columnName, iniutil::name_to_type(typeName), isKey);

						table.m_columns.set(i, column);

						if (isKey == true)
							table.m_keys.add(column);

						column->set_table(table);
					}
				}

				void binary_reader::read_rows(std::istream& stream, binary_table& table, size_t rowCount)
				{
					for (size_t i = 0; i < rowCount; i++)
					{
						binary_row& dataRow = table.m_rows.at(i);

						int length;
						stream.read((char*)&length, sizeof(int));

						dataRow.reserve_fields_ptr(length);
						stream.read(dataRow.fields_ptr(), length);
						dataRow.set_table(table);
						table.m_rows.generate_key(i);
					}
				}

				binary_column::binary_column(const std::string& columnName, const std::type_info& dataType, bool isKey)
					: m_columnName(columnName), m_dataType(dataType), m_iskey(isKey)
				{
#ifdef _DEBUG
					this->typeName = m_dataType.name();
#endif
				}

				binary_column::~binary_column()
				{

				}

				const std::string& binary_column::name() const
				{
					return m_columnName;
				}

				const std::type_info& binary_column::datatype() const
				{
					return m_dataType;
				}

				bool binary_column::is_key() const
				{
					return m_iskey;
				}

				size_t binary_column::index() const
				{
					return m_columnIndex;
				}

				void binary_column::set_index(size_t index)
				{
					m_columnIndex = index;
				}

				itable& binary_column::table() const
				{
					return *m_table;
				}

				void binary_column::set_table(itable& table)
				{
					m_table = &table;
				}

				binary_row::binary_row()
				{

				}

				binary_row::~binary_row()
				{

				}

				const void* binary_row::value_core(const inicolumn& column) const
				{
					static long long nullvalue = 0;
					const int* offsets = (const int*)&m_fields.front();
					int offset = offsets[column.index()];

					const char* valuePtr = &m_fields.front() + offset;
					const std::type_info& typeinfo = column.datatype();

					if (typeinfo == typeid(std::string))
					{
						if (offset == 0)
							return &string_resource::empty_string;
						int id = *(int*)valuePtr;
						return &string_resource::get(id);
					}
					else
					{
						if (offset == 0)
							return &nullvalue;
						return valuePtr;
					}
				}

				bool binary_row::has_value_core(const inicolumn& column) const
				{
					const int* offsets = (const int*)&m_fields.front();
					int offset = offsets[column.index()];
					return offset != 0;
				}

				void binary_row::set_value(const std::string& /*columnName*/, const std::string& /*text*/)
				{
					throw std::invalid_argument("지원되지 않습니다");
				}

				itable& binary_row::table() const
				{
					return *m_table;
				}

				long binary_row::hash() const
				{
					return m_hash;
				}

				void binary_row::reserve_fields_ptr(size_t size)
				{
					m_fields.resize(size, 0);
				}

				char* binary_row::fields_ptr()
				{
					return &m_fields.front();
				}

				void binary_row::set_table(binary_table& table)
				{
					m_table = &table;
				}

				void binary_row::set_hash(long hash)
				{
					m_hash = hash;
				}

				bool binary_row::equals_key(va_list& vl)
				{
					for (inicolumn& item : m_table->m_keys)
					{
						const std::type_info& typeinfo = *va_arg(vl, const std::type_info*);
						if (typeinfo == typeid(bool))
						{
							if (this->value<bool>(item.name()) != !!va_arg(vl, int))
								return false;
						}
						else if (typeinfo == typeid(char))
						{
							if (this->value<char>(item.name()) != (char)va_arg(vl, int))
								return false;
						}
						else if (typeinfo == typeid(unsigned char))
						{
							if (this->value<unsigned char>(item.name()) != (unsigned char)va_arg(vl, int))
								return false;
						}
						else if (typeinfo == typeid(short))
						{
							if (this->value<short>(item.name()) != (short)va_arg(vl, int))
								return false;
						}
						else if (typeinfo == typeid(unsigned short))
						{
							if (this->value<unsigned short>(item.name()) != (unsigned short)va_arg(vl, int))
								return false;
						}
						else if (typeinfo == typeid(int))
						{
							if (this->value<int>(item.name()) != (int)va_arg(vl, int))
								return false;
						}
						else if (typeinfo == typeid(unsigned int))
						{
							if (this->value<unsigned int>(item.name()) != (unsigned int)va_arg(vl, int))
								return false;
						}
						else if (typeinfo == typeid(float))
						{
							if (this->value<float>(item.name()) != (float)va_arg(vl, double))
								return false;
						}
						else if (typeinfo == typeid(double))
						{
							if (this->value<double>(item.name()) != (float)va_arg(vl, double))
								return false;
						}
						else if (typeinfo == typeid(long long))
						{
							if (this->value<long long>(item.name()) != (long long)va_arg(vl, long long))
								return false;
						}
						else if (typeinfo == typeid(unsigned long long))
						{
							if (this->value<unsigned long long>(item.name()) != (unsigned long long)va_arg(vl, long long))
								return false;
						}
						else if (typeinfo == typeid(char*) || typeinfo == typeid(const char*))
						{
							std::string text = va_arg(vl, const char*);
							if (this->value<std::string>(item.name()) != text)
								return false;
						}
						else if (typeinfo == typeid(std::string))
						{
							const std::string& text = *(const std::string*)va_arg(vl, const char*);
							if (this->value<std::string>(item.name()) != text)
								return false;
						}
					}
					return true;
				}

				binary_key_array::binary_key_array()
				{

				}

				binary_key_array::~binary_key_array()
				{

				}

				size_t binary_key_array::size() const
				{
					return m_columns.size();
				}

				inicolumn& binary_key_array::at(size_t index) const
				{
					return *m_columns[index];
				}

				void binary_key_array::add(binary_column* column)
				{
					m_columns.push_back(column);
				}

				binary_column_array::binary_column_array(size_t count)
					: m_columns(count), m_caseSensitive(false)
				{

				}

				binary_column_array::~binary_column_array()
				{
					for (binary_column* item : m_columns)
					{
						delete item;
					}
				}

				size_t binary_column_array::size() const
				{
					return m_columns.size();
				}

				inicolumn& binary_column_array::at(size_t index) const
				{
					return *m_columns[index];
				}

				inicolumn& binary_column_array::at(const std::string& columnName) const
				{
					std::map<std::string, binary_column*>::const_iterator itor = m_nameToColumn.find(conv_string(columnName));
					if (itor == m_nameToColumn.end())
						throw keynotfoundexception(columnName, "columns");
					return *itor->second;
				}

				bool binary_column_array::contains(const std::string& columnName) const
				{
					return m_nameToColumn.find(conv_string(columnName)) != m_nameToColumn.end();
				}

				void binary_column_array::set(size_t index, binary_column* column)
				{
					m_columns[index] = column;
					m_nameToColumn[conv_string(column->name())] = column;
					column->set_index(index);
				}

				void binary_column_array::set_flag(ReadFlag flag)
				{
					m_caseSensitive = (flag & ReadFlag_case_sensitive) != 0;
				}

				std::string binary_column_array::conv_string(const std::string& text) const
				{
					if (m_caseSensitive == true)
						return text;
					return iniutil::to_lower(text);
				}

				binary_row_array::binary_row_array(size_t count)
					: m_rows(count)
				{
					m_keyTorow.get_allocator().allocate(count);
				}

				binary_row_array::~binary_row_array()
				{

				}

				size_t binary_row_array::size() const
				{
					return m_rows.size();
				}

				binary_row& binary_row_array::at(size_t index) const
				{
					const binary_row& row = m_rows[index];
					return const_cast<binary_row&>(row);
				}

				binary_row& binary_row_array::at(size_t index)
				{
					return m_rows[index];
				}

				void binary_row_array::generate_key(size_t index)
				{
					size_t keysize = this->keys_size(), offset = 0;

					if (keysize == 0)
						return;

					binary_row& row = this->at(index);
					const std::collate<char>& coll = std::use_facet< std::collate<char> >(std::locale());
					std::vector<char> fields(keysize);
					for (inicolumn& item : m_table->m_keys)
					{
						const std::type_info& typeinfo = item.datatype();
						if (typeinfo == typeid(bool))
						{
							this->set_field_value(&fields.front(), offset, row.value<bool>(item));
						}
						else if (typeinfo == typeid(char))
						{
							this->set_field_value(&fields.front(), offset, row.value<char>(item));
						}
						else if (typeinfo == typeid(unsigned char))
						{
							this->set_field_value(&fields.front(), offset, row.value<unsigned char>(item));
						}
						else if (typeinfo == typeid(short))
						{
							this->set_field_value(&fields.front(), offset, row.value<short>(item));
						}
						else if (typeinfo == typeid(unsigned short))
						{
							this->set_field_value(&fields.front(), offset, row.value<unsigned short>(item));
						}
						else if (typeinfo == typeid(int))
						{
							this->set_field_value(&fields.front(), offset, row.value<int>(item));
						}
						else if (typeinfo == typeid(unsigned int))
						{
							this->set_field_value(&fields.front(), offset, row.value<unsigned int>(item));
						}
						else if (typeinfo == typeid(long long))
						{
							this->set_field_value(&fields.front(), offset, row.value<long long>(item));
						}
						else if (typeinfo == typeid(unsigned long long))
						{
							this->set_field_value(&fields.front(), offset, row.value<unsigned long long>(item));
						}
						else if (typeinfo == typeid(float))
						{
							this->set_field_value(&fields.front(), offset, row.value<float>(item));
						}
						else if (typeinfo == typeid(std::string))
						{
							const std::string& text = *(const std::string*)row.value_core(item);
							this->set_field_value(&fields.front(), offset, iniutil::get_hash_code(text));
						}
					}

					long hash = coll.hash(&fields.front(), &fields.front() + fields.size());

					row.set_hash(hash);

					m_keyTorow.insert(std::multimap<long, binary_row*>::value_type(hash, &row));
				}

				void binary_row_array::set_table(binary_table& table)
				{
					m_table = &table;
				}

				binary_table& binary_row_array::table() const
				{
					return *m_table;
				}

				binary_row_array::iterator binary_row_array::find_core(size_t count, ...)
				{
					if (count != m_table->m_keys.size())
						throw std::invalid_argument("인자의 갯수가 키의 갯수랑 같지 않습니다.");
					va_list vl;
					size_t keysize = this->keys_size();
					va_start(vl, count);

					size_t offset = 0;
					std::vector<char> fields(keysize);
					const std::collate<char>& coll = std::use_facet< std::collate<char> >(std::locale());

					for (size_t i = 0; i < count; i++)
					{
						const std::type_info& typeinfo = *va_arg(vl, const std::type_info*);
						if (typeinfo == typeid(bool))
						{
							this->set_field_value(&fields.front(), offset, !!va_arg(vl, int));
						}
						else if (typeinfo == typeid(char))
						{
							this->set_field_value(&fields.front(), offset, (char)va_arg(vl, int));
						}
						else if (typeinfo == typeid(unsigned char))
						{
							this->set_field_value(&fields.front(), offset, (unsigned char)va_arg(vl, int));
						}
						else if (typeinfo == typeid(short))
						{
							this->set_field_value(&fields.front(), offset, (short)va_arg(vl, int));
						}
						else if (typeinfo == typeid(unsigned short))
						{
							this->set_field_value(&fields.front(), offset, (unsigned short)va_arg(vl, int));
						}
						else if (typeinfo == typeid(int))
						{
							this->set_field_value(&fields.front(), offset, (int)va_arg(vl, int));
						}
						else if (typeinfo == typeid(unsigned int))
						{
							this->set_field_value(&fields.front(), offset, (unsigned int)va_arg(vl, int));
						}
						else if (typeinfo == typeid(float))
						{
							this->set_field_value(&fields.front(), offset, (float)va_arg(vl, double));
						}
						else if (typeinfo == typeid(double))
						{
							this->set_field_value(&fields.front(), offset, (float)va_arg(vl, double));
						}
						else if (typeinfo == typeid(long long))
						{
							this->set_field_value(&fields.front(), offset, (long long)va_arg(vl, long long));
						}
						else if (typeinfo == typeid(unsigned long long))
						{
							this->set_field_value(&fields.front(), offset, (unsigned long long)va_arg(vl, long long));
						}
						else if (typeinfo == typeid(char*) || typeinfo == typeid(const char*))
						{
							int stringID = iniutil::get_hash_code(va_arg(vl, const char*));
							this->set_field_value(&fields.front(), offset, stringID);
						}
						else if (typeinfo == typeid(std::string))
						{
							const std::string& text = *(const std::string*)va_arg(vl, const char*);
							int stringID = iniutil::get_hash_code(text);
							this->set_field_value(&fields.front(), offset, stringID);
						}
					}
					va_end(vl);

					long hash = coll.hash(&fields.front(), &fields.front() + fields.size());
					std::pair <std::multimap<long, binary_row*>::iterator, std::multimap<long, binary_row*>::iterator> ret = m_keyTorow.equal_range(hash);

					size_t len = std::distance(ret.first, ret.second);

					if (len == 1)
					{
						size_t index = ret.first->second - (binary_row*)&m_rows.front();
						return iterator(this, index);
					}

					for (std::multimap<long, binary_row*>::iterator itor = ret.first; itor != ret.second; ++itor)
					{
						va_list vl1;
						va_start(vl1, count);
						if (itor->second->equals_key(vl1) == true)
						{
							size_t index = itor->second - (binary_row*)&m_rows.front();
							return iterator(this, index);
						}
						va_end(vl1);
					}

					return iterator(this);
				}

				size_t binary_row_array::keys_size() const
				{
					size_t size = 0;

					for (inicolumn& item : m_table->m_keys)
					{
						const std::type_info& typeinfo = item.datatype();
#ifdef _DEBUG
						const char* name = typeinfo.name();
#endif

						if (typeinfo == typeid(bool))
						{
							size += sizeof(int);
						}
						else if (typeinfo == typeid(char))
						{
							size += sizeof(int);
						}
						else if (typeinfo == typeid(unsigned char))
						{
							size += sizeof(int);
						}
						else if (typeinfo == typeid(short))
						{
							size += sizeof(int);
						}
						else if (typeinfo == typeid(unsigned short))
						{
							size += sizeof(int);
						}
						else if (typeinfo == typeid(int))
						{
							size += sizeof(int);
						}
						else if (typeinfo == typeid(unsigned int))
						{
							size += sizeof(int);
						}
						else if (typeinfo == typeid(float))
						{
							size += sizeof(double);
						}
						else if (typeinfo == typeid(long long))
						{
							size += sizeof(long long);
						}
						else if (typeinfo == typeid(unsigned long long))
						{
							size += sizeof(long long);
						}
						else if (typeinfo == typeid(std::string))
						{
							size += sizeof(int);
						}
					}

					return size;
				}

				binary_table::binary_table(binary_reader* reader, size_t columnCount, size_t rowCount)
					: m_columns(columnCount), m_rows(rowCount)
				{
					this->m_reader = reader;
					this->m_rows.set_table(*this);
				}

				std::string binary_table::category() const
				{
					return m_categoryName;
				}

				std::string binary_table::name() const
				{
					return m_tableName;
				}

				size_t binary_table::index() const
				{
					return m_index;
				}

				std::string binary_table::hash_value() const
				{
					return m_hashValue;
				}

				void binary_table::set_index(size_t index)
				{
					m_index = index;
				}

				idataset& binary_table::dataset() const
				{
					return *m_reader;
				}

				binary_table::~binary_table()
				{

				}

				binary_table_array::binary_table_array(binary_reader& reader)
					: m_reader(reader)
				{

				}

				binary_table_array::~binary_table_array()
				{
					for (binary_table* item : m_tables)
					{
						delete item;
					}
				}

				size_t binary_table_array::size() const
				{
					return m_tables.size();
				}

				itable& binary_table_array::at(size_t index) const throw()
				{
					itable* table = m_tables.at(index);
					if (table == NULL)
						return *const_cast<binary_table_array*>(this)->m_reader.read_table(index);
					return *table;
				}

				itable& binary_table_array::at(const std::string& tableName) const throw()
				{
					std::map<std::string, binary_table*>::const_iterator itor = m_nameToTable.find(conv_string(tableName));
					if (itor == m_nameToTable.end())
					{
						return *const_cast<binary_table_array*>(this)->m_reader.read_table(conv_string(tableName));
					}
					return *itor->second;
				}

				bool binary_table_array::contains(const std::string& tableName) const
				{
					return m_nameToTable.find(conv_string(tableName)) != m_nameToTable.end();
				}

				void binary_table_array::set(size_t index, binary_table* table)
				{
					std::string tableName;
					m_nameToTable[conv_string(table->name())] = table;
					m_tables[index] = table;
					dynamic_cast<binary_table*>(table)->set_index(index);

#ifdef _DEBUG
					//std::cout << table->name() << " is loaded : " << index << std::endl;
#endif
				}

				void binary_table_array::set_size(const std::vector<table_index>& indexes)
				{
					m_tables.assign(indexes.size(), NULL);

					m_tableNames.reserve(indexes.size());
					for (std::vector<table_index>::const_iterator itor = indexes.begin(); itor != indexes.end(); itor++)
					{
						const std::string& tableName = string_resource::get(itor->tableName);
						m_tableNames.push_back(tableName);
					}
				}

				void binary_table_array::set_flag(ReadFlag flag)
				{
					m_caseSensitive = (flag & ReadFlag_case_sensitive) != 0;
				}

				std::string binary_table_array::conv_string(const std::string& text) const
				{
					if (m_caseSensitive == true)
						return text;
					return iniutil::to_lower(text);
				}

				const itableNameArray& binary_table_array::names() const
				{
					return m_tableNames;
				}

				bool binary_table_array::is_table_loaded(const std::string& tableName) const
				{
					std::map<std::string, binary_table*>::const_iterator itor = m_nameToTable.find(conv_string(tableName));
					return itor != m_nameToTable.end();
				}

				void binary_table_array::load_table(const std::string& tableName)
				{
					if (this->is_table_loaded(tableName) == true)
						return;

					m_reader.read_table(conv_string(tableName));
				}

				void binary_table_array::release_table(const std::string& tableName)
				{
					std::map<std::string, binary_table*>::const_iterator itor = m_nameToTable.find(conv_string(tableName));
					if (itor == m_nameToTable.end())
						return;
					binary_table* table = itor->second;
					m_nameToTable.erase(itor->first);
					m_tables[table->index()] = nullptr;
					delete table;
				}
			} /*namespace binary*/
		} /*namespace internal*/
} /*namespace reader*/
} /*namespace CremaCode*/
