// OpenH264Lib.h

#pragma once

using namespace System;

namespace OpenH264Lib {

	public ref class Encoder
	{
	private:
		int num_of_frames;
		int keyframe_interval;
		int buffer_size;
		unsigned char *i420_buffer;

		// �}�l�[�W�R�[�h�̃N���X�̓l�C�e�B�u�R�[�h�̃N���X�������o�Ɏ����Ƃ͂ł��Ȃ��B���̑���Ƀ|�C���^���g���B
		// �R���X�g���N�^�ŃI�u�W�F�N�g�𐶐����A�t�@�C�i���C�U�ŉ������B�f�X�g���N�^�ł͉�������t�@�C�i���C�U���Ăяo�������B(�K���Ă΂��Ƃ͌���Ȃ�����)
		// �����̃|�C���^�̓}�l�[�W�q�[�v��ɑ��݂��邽�߁A�����Z�q�ŃA�h���X���擾���邱�Ƃ͂ł��Ȃ��Bpin_ptr���g���ăA�h���X���Œ肵�ČĂяo���K�v������B
		ISVCEncoder* encoder;
		SSourcePicture* pic;
		SFrameBSInfo* bsi;

	private:
		typedef int(__stdcall *WelsCreateSVCEncoderFunc)(ISVCEncoder** ppEncoder);
		WelsCreateSVCEncoderFunc CreateEncoderFunc;
		typedef void(__stdcall *WelsDestroySVCEncoderFunc)(ISVCEncoder* ppEncoder);
		WelsDestroySVCEncoderFunc DestroyEncoderFunc;

	private:
		~Encoder(); // �f�X�g���N�^
		!Encoder(); // �t�@�C�i���C�U
	public:
		Encoder(String ^dllName);

	public:
		enum class FrameType { Invalid, IDR, I, P, Skip, IPMixed };
		delegate void OnEncodeCallback(array<Byte> ^data, int length, FrameType keyFrame);
		int Setup(int width, int height, int bps, float fps, float keyFrameInterval, OnEncodeCallback ^onEncode);

		[Obsolete("timestamp argument is unnecessary. use Encode(Bitmap) instead.")]
		int Encode(System::Drawing::Bitmap ^bmp, float timestamp);

		int Encode(System::Drawing::Bitmap ^bmp);
		int Encode(array<Byte> ^i420);
		int Encode(unsigned char *i420);

	private:
		void OnEncode(const SFrameBSInfo% info);
		OnEncodeCallback^ OnEncodeFunc;

	private:
		static unsigned char* BitmapToRGBA(System::Drawing::Bitmap^ bmp, int width, int height);
		static unsigned char* RGBAtoYUV420Planar(unsigned char *rgba, int width, int height);
	};
}
