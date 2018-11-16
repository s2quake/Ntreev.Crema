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

#pragma once

//#define _CREMA_READER_VER 209
//#define _IGNORE_BOOST

//#ifdef _MSC_VER
//#ifdef _WINDLL
//#define __declspec(dllexport)
//#else
//#define 
//#endif
//
//#if _MSC_VER < 1700
////#define nullptr NULL
//#endif
//#else
////#define 
////#define
////#define nullptr NULL
//#define _IGNORE_BOOST
//
//#include <string>
//namespace std
//{
//	typedef basic_string<wchar_t, char_traits<wchar_t>, allocator<wchar_t> > wstring;
//}
//#endif

#include <string>
#include <algorithm>
#include <map>
#include <string>
#include <typeinfo>
#include <vector>
#include <list>
#include <iostream>
#include <sstream>
#include <stdexcept>
#include <type_traits>
#include <cstdarg>

namespace CremaCode {
	namespace reader {
		class keynotfoundexception : public std::out_of_range
		{
		public:
			keynotfoundexception(const std::string& key, const std::string& container_name);

		private:
			std::string format_string(const std::string& key, const std::string& container_name);

		private:
		};

		class idataset;

		template<typename _inicontainer, typename _initype>
		class cremaiterator : public std::iterator<std::input_iterator_tag, _initype>
		{
		public:
			cremaiterator(_inicontainer* c) : i(c->size()) { }
			cremaiterator(_inicontainer* c, size_t i) : i(i), c(c) { }
			cremaiterator(const cremaiterator& mit) : i(mit.i), c(mit.c) { }

			cremaiterator& operator++() { i = std::min(c->size(), ++i); return *this; }
			cremaiterator operator++(int) { cremaiterator tmp(*this); operator++(); return tmp; }
			bool operator==(const cremaiterator& rhs) { return i == rhs.i; }
			bool operator!=(const cremaiterator& rhs) { return i != rhs.i; }
			_initype& operator*() { return c->at(i); }
			_initype* operator->() { return &c->at(i); }

		private:
			size_t i;
			_inicontainer* c;
		};

		template<typename _inicontainer, typename _initype>
		class const_cremaiterator : public std::iterator<std::input_iterator_tag, _initype>
		{
		public:
			const_cremaiterator(const _inicontainer* c) : i(c->size()) { }
			const_cremaiterator(const _inicontainer* c, size_t i) : i(i), c(c) { }
			const_cremaiterator(const const_cremaiterator& mit) : i(mit.i), c(mit.c) { }

			const_cremaiterator& operator++() { i = std::min(c->size(), ++i); return *this; }
			const_cremaiterator operator++(int) { const_cremaiterator tmp(*this); operator++(); return tmp; }
			bool operator==(const const_cremaiterator& rhs) { return i == rhs.i; }
			bool operator!=(const const_cremaiterator& rhs) { return i != rhs.i; }
			const _initype& operator*() { return c->at(i); }
			const _initype* operator->() { return &c->at(i); }
		private:
			size_t i;
			const _inicontainer* c;
		};

		class itable;

		class inicolumn
		{
		public:
			inicolumn() {};

			virtual const std::string& name() const = 0;
			virtual const std::type_info& datatype() const = 0;
			virtual bool is_key() const = 0;
			virtual size_t index() const = 0;
			virtual itable& table() const = 0;

		protected:
			virtual ~inicolumn() {};

#ifdef _DEBUG
		protected:
			const char* typeName;
#endif

		};

		class irow
		{
		public:
			virtual ~irow() {};

			virtual void set_value(const std::string& columnName, const std::string& text) = 0;
			virtual itable& table() const = 0;
			virtual long hash() const = 0;

			template<typename T>
			const T& value(const std::string& columnName) const;

			template<typename T>
			const T& value(size_t index) const;

			template<typename T>
			const T& value(const inicolumn& column) const;

			bool has_value(const std::string& columnName) const;
			bool has_value(size_t index) const;
			bool has_value(const inicolumn& column) const;

			bool to_boolean(const std::string& columnName) const;
			bool to_boolean(size_t index) const;
			bool to_boolean(const inicolumn& column) const;

			const std::string& to_string(const std::string& columnName) const;
			const std::string& to_string(size_t index) const;
			const std::string& to_string(const inicolumn& column) const;

			float to_single(const std::string& columnName) const;
			float to_single(size_t index) const;
			float to_single(const inicolumn& column) const;

			double to_double(const std::string& columnName) const;
			double to_double(size_t index) const;
			double to_double(const inicolumn& column) const;

			char to_int8(const std::string& columnName) const;
			char to_int8(size_t index) const;
			char to_int8(const inicolumn& column) const;

			unsigned char to_uint8(const std::string& columnName) const;
			unsigned char to_uint8(size_t index) const;
			unsigned char to_uint8(const inicolumn& column) const;

			short to_int16(const std::string& columnName) const;
			short to_int16(size_t index) const;
			short to_int16(const inicolumn& column) const;

			unsigned short to_uint16(const std::string& columnName) const;
			unsigned short to_uint16(size_t index) const;
			unsigned short to_uint16(const inicolumn& column) const;

			int to_int32(const std::string& columnName) const;
			int to_int32(size_t index) const;
			int to_int32(const inicolumn& column) const;

			unsigned int to_uint32(const std::string& columnName) const;
			unsigned int to_uint32(size_t index) const;
			unsigned int to_uint32(const inicolumn& column) const;

			long long to_int64(const std::string& columnName) const;
			long long to_int64(size_t index) const;
			long long to_int64(const inicolumn& column) const;

			unsigned long long to_uint64(const std::string& columnName) const;
			unsigned long long to_uint64(size_t index) const;
			unsigned long long to_uint64(const inicolumn& column) const;

			time_t to_datetime(const std::string& columnName) const;
			time_t to_datetime(size_t index) const;
			time_t to_datetime(const inicolumn& column) const;

			long long to_duration(const std::string& columnName) const;
			long long to_duration(size_t index) const;
			long long to_duration(const inicolumn& column) const;

		protected:
			virtual const void* value_core(const inicolumn& column) const = 0;
			virtual bool has_value_core(const inicolumn& column) const = 0;
		};

		class inikey_array
		{
		public:
			typedef cremaiterator<inikey_array, inicolumn> iterator;
			typedef const_cremaiterator<inikey_array, inicolumn> const_iterator;

			inikey_array() {};
			virtual ~inikey_array() {};

			virtual size_t size() const = 0;
			virtual inicolumn& at(size_t index) const = 0;

			inicolumn& operator [] (size_t index) const;

			iterator begin() { return iterator(this, 0); }
			iterator end() { return iterator(this); }
			const_iterator begin() const { return const_iterator(this, 0); }
			const_iterator end() const { return const_iterator(this); }
		};

		class icolumn_array
		{
		public:
			typedef cremaiterator<icolumn_array, inicolumn> iterator;
			typedef const_cremaiterator<icolumn_array, inicolumn> const_iterator;

			icolumn_array() {};
			virtual ~icolumn_array() {};

			virtual size_t size() const = 0;
			virtual inicolumn& at(size_t index) const = 0;
			virtual inicolumn& at(const std::string& columnName) const = 0;
			virtual bool contains(const std::string& columnName) const = 0;

			inicolumn& operator [] (const std::string& columnName) const;
			inicolumn& operator [] (size_t index) const;

			iterator begin() { return iterator(this, 0); }
			iterator end() { return iterator(this); }
			const_iterator begin() const { return const_iterator(this, 0); }
			const_iterator end() const { return const_iterator(this); }
		};

		class irow_array
		{
		public:
			typedef cremaiterator<irow_array, irow> iterator;
			typedef const_cremaiterator<irow_array, irow> const_iterator;

			irow_array() {};
			virtual ~irow_array() {};

			virtual size_t size() const = 0;
			virtual irow& at(size_t index) const = 0;

			irow& operator [] (size_t index) const;

			iterator begin() { return iterator(this, 0); }
			iterator end() { return iterator(this); }
			const_iterator begin() const { return const_iterator(this, 0); }
			const_iterator end() const { return const_iterator(this); }

			template<typename key_type>
			iterator find(key_type key_value)
			{
				this->type_validation<key_type>();
				return this->find_core(1, &typeid(key_type), key_value);
			}

			template<typename key_type1, typename key_type2>
			iterator find(key_type1 key_value1, key_type2 key_value2)
			{
				this->type_validation<key_type1>();
				this->type_validation<key_type2>();
				return this->find_core(2,
					&typeid(key_type1), key_value1,
					&typeid(key_type2), key_value2);
			}

			template<typename key_type1, typename key_type2, typename key_type3>
			iterator find(key_type1 key_value1, key_type2 key_value2, key_type3 key_value3)
			{
				this->type_validation<key_type1>();
				this->type_validation<key_type2>();
				this->type_validation<key_type3>();
				return this->find_core(3,
					&typeid(key_type1), key_value1,
					&typeid(key_type2), key_value2,
					&typeid(key_type3), key_value3);
			}

			template<typename key_type1, typename key_type2, typename key_type3, typename key_type4>
			iterator find(key_type1 key_value1, key_type2 key_value2, key_type3 key_value3, key_type4 key_value4)
			{
				this->type_validation<key_type1>();
				this->type_validation<key_type2>();
				this->type_validation<key_type3>();
				this->type_validation<key_type4>();
				return this->find_core(4,
					&typeid(key_type1), key_value1,
					&typeid(key_type2), key_value2,
					&typeid(key_type3), key_value3,
					&typeid(key_type4), key_value4);
			}

		protected:
			virtual iterator find_core(size_t count, ...) = 0;

		private:
			template<typename type>
			void type_validation()
			{
#ifdef _MSC_VER
				bool value = std::is_integral<type>::value ||
					std::is_floating_point<type>::value ||
					typeid(type) == typeid(const char*) ||
					typeid(type) == typeid(char*) ||
					typeid(type) == typeid(std::string) ||
					typeid(type) == typeid(const std::string);
				if (value == false)
				{
					std::ostringstream stream;
					const char* werewr = typeid(type).name();
					stream << typeid(type).name() << "은(는) 올바른 타입이 아닙니다.";
					throw std::invalid_argument(stream.str());
				}
#endif
			}
		};

		class itable
		{
		public:
			virtual std::string category() const = 0;
			virtual std::string name() const = 0;
			virtual size_t index() const = 0;
			virtual std::string hash_value() const = 0;

			virtual const inikey_array& keys() const = 0;
			virtual const icolumn_array& columns() const = 0;
			virtual const irow_array& rows() const = 0;

			virtual idataset& dataset() const = 0;

		protected:
			itable & operator=(const itable&) { return *this; }
			itable() {};
			virtual ~itable() {};
		};

		typedef std::vector<std::string> itableNameArray;

		class itable_array
		{
		public:
			typedef cremaiterator<itable_array, itable> iterator;
			typedef const_cremaiterator<itable_array, itable> const_iterator;

			itable_array() {};
			virtual ~itable_array() {};

			virtual size_t size() const = 0;
			virtual itable& at(size_t index) const throw() = 0;
			virtual itable& at(const std::string& tableName) const throw() = 0;
			virtual bool contains(const std::string& tableName) const = 0;
			virtual const itableNameArray& names() const = 0;
			virtual bool is_table_loaded(const std::string& tableName) const = 0;
			virtual void load_table(const std::string& tableName) = 0;
			virtual void release_table(const std::string& tableName) = 0;

			itable& operator [] (const std::string& tableName) const;
			itable& operator [] (size_t index) const;

			iterator begin() { return iterator(this, 0); }
			iterator end() { return iterator(this); }
			const_iterator begin() const { return const_iterator(this, 0); }
			const_iterator end() const { return const_iterator(this); }
		};

		template<typename T>
		const T& irow::value(const std::string& columnName) const
		{
			return this->value<T>(this->table().columns().at(columnName));
		}

		template<typename T>
		const T& irow::value(size_t index) const
		{
			return this->value<T>(this->table().columns().at(index));
		}

		template<typename T>
		const T& irow::value(const inicolumn& column) const
		{
			if (column.datatype() != typeid(T))
			{
				std::ostringstream stream;
				stream << column.datatype().name() << " 에서 " << typeid(T).name() << " 으로 변환할 수 없습니다. ";

				throw std::invalid_argument(stream.str());
			}
			return *(T*)this->value_core(column);
		}

		class idataset
		{
		public:
			virtual const itable_array& tables() const = 0;
			virtual const std::string& name() const = 0;
			virtual const std::string& revision() const = 0;
			virtual const std::string& types_hash_value() const = 0;
			virtual const std::string& tables_hash_value() const = 0;
			virtual const std::string& tags() const = 0;
		};

		enum ReadFlag
		{
			ReadFlag_none = 0,
			ReadFlag_lazy_loading = 1,
			ReadFlag_case_sensitive = 2,

			ReadFlag_mask = 0xff,
		};

		enum DataLocation
		{
			DataLocation_both,
			DataLocation_server,
			DataLocation_client,
		};

		class CremaReader : public idataset
		{
		public:
	#ifndef _IGNORE_BOOST
			//static CremaReader& read(const std::string& address, int port, const std::string& name = "default", ReadFlag flag = ReadFlag_none);
			static CremaReader& read(const std::string& address, int port, const std::string& database, DataLocation datalocation, ReadFlag flag = ReadFlag_none);
	#endif
			static CremaReader& read(const std::string& filename, ReadFlag flag = ReadFlag_none);
			static CremaReader& read(std::istream& stream, ReadFlag flag = ReadFlag_none);

			static CremaReader& ReadFromFile(const std::string& filename, ReadFlag flag = ReadFlag_none)
			{
				return read(filename, flag);
			}

			virtual void destroy() = 0;

			//virtual const itable_array& tables() const = 0;

		protected:
			CremaReader();
			virtual ~CremaReader();
			CremaReader& operator=(const CremaReader&) { return *this; }
			virtual void read_core(std::istream& stream, ReadFlag flag) = 0;

		private:
			std::istream* m_stream;
		};

		class enum_info
		{
		public:
			enum_info(bool isFlag);
			~enum_info();

			void add(const std::string& name, int value);
			int value(const std::string& name) const;
			std::string name(int value) const;
			std::vector<std::string> names() const;
			int parse(const std::string& text) const;
			std::string to_string(int value) const;
			bool is_flag() const;

		private:
			std::map<std::string, int>* m_values;
			std::map<int, std::string>* m_names;
			bool m_isflag;
		};

		class enum_util
		{
		public:
			static int parse(const std::type_info& typeinfo, const std::string& text);
			static void add(const std::type_info& typeinfo, const enum_info* penum_info);
			static bool contains(const std::type_info& typeinfo);
			static std::string name(const std::type_info& typeinfo, int value);
			static std::vector<std::string> names(const std::type_info& typeinfo);
			static std::string to_string(const std::type_info& typeinfo, int value);
			static bool is_flag(const std::type_info& typeinfo);

			template<typename T>
			static std::string name(T value)
			{
				return name(typeid(T), (int)value);
			}

			static std::string name(int /*value*/)
			{
				throw std::invalid_argument("타입을 지정하세요");
			}

			template<typename T>
			static std::vector<std::string> names()
			{
				return names(typeid(T));
			}

			template<typename T>
			static T parse(const std::string& text)
			{
				return (T)parse(typeid(T), text);
			}
		};

		class convert
		{
		public:
			static short to_int16(const std::string& text);
			static int to_int32(const std::string& text);
			static long long to_int64(const std::string& text);
			static unsigned int to_uint32(const std::string& text);
			static float to_float(const std::string& text);
			static bool to_boolean(const std::string& text);
		};

		class iniutil
		{
		public:
			static std::string utf8_to_string(const char* text);
			static std::wstring string_to_wstring(const std::string& text);
			static const std::type_info& name_to_type(const std::string& typeName);
			static int get_hash_code(const std::string& text);
			static std::string to_lower(const std::string& text);

			static int get_type_size(const std::type_info& typeinfo);

			template<typename key_type>
			static long generate_hash(key_type key_value)
			{
				size_t size = keys_size(1, &typeid(key_type));
				return generate_hash_core(1, size, &typeid(key_type), key_value);
			}

			template<typename key_type1, typename key_type2>
			static long generate_hash(key_type1 key_value1, key_type2 key_value2)
			{
				size_t size = keys_size(2, &typeid(key_type1), &typeid(key_type2));
				return generate_hash_core(2, size,
					&typeid(key_type1), key_value1,
					&typeid(key_type2), key_value2);
			}

			template<typename key_type1, typename key_type2, typename key_type3>
			static long generate_hash(key_type1 key_value1, key_type2 key_value2, key_type3 key_value3)
			{
				size_t size = keys_size(3, &typeid(key_type1), &typeid(key_type2), &typeid(key_type3));
				return generate_hash_core(3, size,
					&typeid(key_type1), key_value1,
					&typeid(key_type2), key_value2,
					&typeid(key_type3), key_value3);
			}

			template<typename key_type1, typename key_type2, typename key_type3, typename key_type4>
			static long generate_hash(key_type1 key_value1, key_type2 key_value2, key_type3 key_value3, key_type4 key_value4)
			{
				size_t size = keys_size(4, &typeid(key_type1), &typeid(key_type2), &typeid(key_type3), &typeid(key_type4));
				return generate_hash_core(4, size,
					&typeid(key_type1), key_value1,
					&typeid(key_type2), key_value2,
					&typeid(key_type3), key_value3,
					&typeid(key_type4), key_value4);
			}

			template<typename key_type1, typename key_type2, typename key_type3, typename key_type4, typename key_type5>
			static long generate_hash(key_type1 key_value1, key_type2 key_value2, key_type3 key_value3, key_type4 key_value4, key_type5 key_value5)
			{
				size_t size = keys_size(5, &typeid(key_type1), &typeid(key_type2), &typeid(key_type3), &typeid(key_type4), &typeid(key_type5));
				return generate_hash_core(5, size,
					&typeid(key_type1), key_value1,
					&typeid(key_type2), key_value2,
					&typeid(key_type3), key_value3,
					&typeid(key_type4), key_value4,
					&typeid(key_type5), key_value5);
			}

		private:
			static long generate_hash_core(size_t count, size_t keysize, ...);
			static size_t keys_size(size_t count, ...);

			template<typename _type>
			static void set_field_value(const char* buffer, size_t& offset, _type value)
			{
				_type* value_ptr = (_type*)(buffer + offset);
				*value_ptr = value;
				offset += sizeof(_type);
			}
		};

		namespace internal {

			class internal_util
			{
			public:
				internal_util();
				~internal_util();

				template<typename _type>
				static void set_field_value(const char* buffer, size_t& offset, _type value)
				{
					_type* value_ptr = (_type*)(buffer + offset);
					*value_ptr = value;
					offset += sizeof(_type);
				}

				std::list<CremaReader*> m_readers;


			};

			class string_resource
			{
			public:
				static void read(std::istream& stream);
				static const std::string& get(int id);

				static void add_ref();
				static void remove_ref();

			public:
				static std::string empty_string;
				static std::string invalid_type;
			private:
				static std::map<int, std::string> m_strings;
				static int m_ref;
			};

			namespace binary
			{
				const int magic_value_obsolete = 0x6cfc4a14;
				const int magic_value = 0x03050000;

				struct table_header
				{
					int magicValue;
					int hashValue;
					long long modifiedTime;
					long long tableInfoOffset;
					long long columnsOffset;
					long long rowsOffset;
					long long stringResourcesOffset;
					long long userOffset;
				};

				struct table_info
				{
					int tableName;
					int categoryName;
					int columnCount;
					int rowCount;
				};

				struct column_info
				{
					int columnName;
					int dataType;
					int iskey;
				};

				struct file_header
				{
					int magicValue;
					int revision;
					int typesHashValue;
					int tablesHashValue;
					int tags;
					int reserved;
					int tableCount;
					int name;
					long long indexOffset;
					long long tablesOffset;
					long long stringResourcesOffset;
				};

				struct table_index
				{
					int tableName;
					int dummy;
					long long offset;
				};

				class binary_table;
				class binary_reader;



				class binary_column : public inicolumn
				{
				public:
					binary_column(const std::string& columnName, const std::type_info& dataType, bool iskey);
					virtual ~binary_column();

					virtual const std::string& name() const;
					virtual const std::type_info& datatype() const;
					virtual bool is_key() const;
					virtual size_t index() const;

					virtual itable& table() const;

					void set_table(itable& table);
					void set_index(size_t index);

					binary_column& operator=(const binary_column&) { return *this; }

				private:
					std::string m_columnName;
					const std::type_info& m_dataType;
					bool m_iskey;
					size_t m_columnIndex;
					itable* m_table;
				};

				class binary_row : public irow
				{
				public:
					binary_row();
					virtual ~binary_row();

					virtual const void* value_core(const inicolumn& column) const;
					virtual bool has_value_core(const inicolumn& column) const;
					virtual void set_value(const std::string& columnName, const std::string& text);
					virtual itable& table() const;
					virtual long hash() const;

					void reserve_fields_ptr(size_t size);
					char* fields_ptr();
					void set_table(binary_table& table);
					void set_hash(long hash);
					bool equals_key(va_list& vl);

				private:
					std::vector<char> m_fields;
					binary_table* m_table;
					long m_hash;
				};

				class binary_column_array : public icolumn_array
				{
				public:
					binary_column_array(size_t count);
					virtual ~binary_column_array();

					virtual size_t size() const;
					virtual inicolumn& at(size_t index) const;
					virtual inicolumn& at(const std::string& columnName) const;
					virtual bool contains(const std::string& columnName) const;

					void set(size_t index, binary_column* column);
					void set_flag(ReadFlag flag);

				private:
					std::string conv_string(const std::string& text) const;

				private:
					std::vector<binary_column*> m_columns;
					std::map<std::string, binary_column*> m_nameToColumn;
					bool m_caseSensitive;
				};

				class binary_key_array : public inikey_array
				{
				public:
					binary_key_array();
					virtual ~binary_key_array();

					virtual size_t size() const;
					virtual inicolumn& at(size_t index) const;

					void add(binary_column* column);

				private:
					std::vector<binary_column*> m_columns;
				};

				class binary_row_array : public irow_array
				{
				public:
					binary_row_array(size_t count);
					virtual ~binary_row_array();

					virtual size_t size() const;
					virtual binary_row& at(size_t index) const;

					binary_row& at(size_t index);
					void generate_key(size_t index);
					void set_table(binary_table& table);
					binary_table& table() const;

					iterator find_core(size_t count, ...);

				private:
					size_t keys_size() const;
					template<typename _type>
					void set_field_value(const char* buffer, size_t& offset, _type value)
					{
						_type* value_ptr = (_type*)(buffer + offset);
						*value_ptr = value;
						offset += sizeof(_type);
					}

				private:
					std::vector<binary_row> m_rows;
					std::multimap<long, binary_row*> m_keyTorow;
					binary_table* m_table;
				};

				class binary_table : public itable
				{
				public:
					binary_table(binary_reader* reader, size_t columnCount, size_t rowCount);
					virtual ~binary_table();

					virtual std::string category() const;
					virtual std::string name() const;
					virtual size_t index() const;
					virtual std::string hash_value() const;

					void set_index(size_t index);

					virtual const inikey_array& keys() const { return m_keys; }
					virtual const icolumn_array& columns() const { return m_columns; }
					virtual const irow_array& rows() const { return m_rows; }

					virtual idataset& dataset() const;

					binary_key_array m_keys;
					binary_column_array m_columns;
					binary_row_array m_rows;

				private:
					std::string m_tableName;
					std::string m_categoryName;
					size_t m_index;
					std::string m_hashValue;
					binary_reader* m_reader;

					friend class binary_reader;
				};

				class binary_table_array : public itable_array
				{
				public:
					binary_table_array(binary_reader& reader);
					virtual ~binary_table_array();

					virtual size_t size() const;
					virtual itable& at(size_t index) const throw();
					virtual itable& at(const std::string& tableName) const throw();
					virtual bool contains(const std::string& tableName) const;
					virtual const itableNameArray& names() const;
					virtual bool is_table_loaded(const std::string& tableName) const;
					virtual void load_table(const std::string& tableName);
					virtual void release_table(const std::string& tableName);


					void set(size_t index, binary_table* dataTable);
					void set_size(const std::vector<table_index>& indexes);
					void set_flag(ReadFlag flag);

					binary_table_array& operator=(const binary_table_array&) { return *this; }

				private:
					std::string conv_string(const std::string& text) const;

				private:
					std::map<std::string, binary_table*> m_nameToTable;
					std::vector<binary_table*> m_tables;
					itableNameArray m_tableNames;
					binary_reader& m_reader;
					bool m_caseSensitive;
				};

				class binary_reader : public CremaReader
				{
				public:
					binary_reader();
					virtual ~binary_reader();

					virtual void read_core(std::istream& stream, ReadFlag flag);
					virtual void destroy();

					binary_table* read_table(const std::string& tableName);
					binary_table* read_table(size_t index);

					virtual const itable_array& tables() const { return m_tables; }
					virtual const std::string& name() const { return m_name; }
					virtual const std::string& revision() const { return m_revision; }
					virtual const std::string& types_hash_value() const { return m_typesHashValue; };
					virtual const std::string& tables_hash_value() const { return m_tablesHashValue; };
					virtual const std::string& tags() const { return m_tags; };

					binary_table_array m_tables;

				private:
					binary_table * read_table(std::istream& stream, std::streamoff offset, ReadFlag flag);
					void read_columns(std::istream& stream, binary_table& dataTable, size_t columnCount, ReadFlag flag);
					void read_rows(std::istream& stream, binary_table& dataTable, size_t rowCount);

					class findif
					{
					public:
						findif(ReadFlag flag, const std::string& tableName) : m_tableName(tableName), m_flag(flag) {}
						bool operator() (const table_index& index);
					private:
						std::string m_tableName;
						ReadFlag m_flag;
					};

				private:
					std::istream* m_stream;
					std::vector<table_index> m_tableIndexes;
					ReadFlag m_flag;
					std::string m_name;
					std::string m_revision;
					std::string m_typesHashValue;
					std::string m_tablesHashValue;
					std::string m_tags;
				};

			} /*namespace binary*/
		} /*namespace internal*/
	} /*namespace reader*/
} /*namespace CremaCode*/

