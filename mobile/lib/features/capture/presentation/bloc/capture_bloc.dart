import 'dart:io';
import 'package:dio/dio.dart';
import 'package:equatable/equatable.dart';
import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http_parser/http_parser.dart';
import '../../../../core/network/api_client.dart';

// ─── Shared Capture Entities ──────────────────────────────────────────────────
class CaptureResult extends Equatable {
  final String merchant;
  final double amount;
  final String currency;
  final String category;
  final String categoryName;
  final double confidenceScore;
  final String captureSource;

  const CaptureResult({
    required this.merchant,
    required this.amount,
    required this.currency,
    required this.category,
    required this.categoryName,
    required this.confidenceScore,
    required this.captureSource,
  });

  @override
  List<Object?> get props => [merchant, amount, captureSource];
}

// ─── Events ──────────────────────────────────────────────────────────────────
sealed class CaptureEvent extends Equatable {
  @override List<Object?> get props => [];
}

class VoiceRecordingStarted extends CaptureEvent {}
class VoiceRecordingStopped extends CaptureEvent {
  final String audioFilePath;
  VoiceRecordingStopped(this.audioFilePath);
  @override List<Object?> get props => [audioFilePath];
}

class OcrImageSelected extends CaptureEvent {
  final File imageFile;
  OcrImageSelected(this.imageFile);
  @override List<Object?> get props => [imageFile.path];
}

class QrScanned extends CaptureEvent {
  final String qrContent;
  QrScanned(this.qrContent);
  @override List<Object?> get props => [qrContent];
}

class SmsParseRequested extends CaptureEvent {
  final String smsBody;
  final String sender;
  SmsParseRequested({required this.smsBody, required this.sender});
  @override List<Object?> get props => [smsBody, sender];
}

class ManualEntrySubmitted extends CaptureEvent {
  final String merchant;
  final double amount;
  final String? category;
  final String? notes;
  ManualEntrySubmitted({required this.merchant, required this.amount, this.category, this.notes});
  @override List<Object?> get props => [merchant, amount];
}

class CaptureConfirmed extends CaptureEvent {}
class CaptureCancelled extends CaptureEvent {}

// ─── States ──────────────────────────────────────────────────────────────────
sealed class CaptureState extends Equatable {
  @override List<Object?> get props => [];
}

class CaptureIdle extends CaptureState {}
class CaptureRecording extends CaptureState {}  // mic is live
class CaptureProcessing extends CaptureState {} // sending to API
class CapturePreview extends CaptureState {
  final CaptureResult result;
  CapturePreview(this.result);
  @override List<Object?> get props => [result];
}
class CaptureSuccess extends CaptureState {}
class CaptureError extends CaptureState {
  final String message;
  CaptureError(this.message);
  @override List<Object?> get props => [message];
}

// ─── BLoC ─────────────────────────────────────────────────────────────────────
class CaptureBloc extends Bloc<CaptureEvent, CaptureState> {
  final ApiClient _apiClient;

  CaptureBloc(this._apiClient) : super(CaptureIdle()) {
    on<VoiceRecordingStopped>(_onVoiceRecordingStopped);
    on<OcrImageSelected>(_onOcrImageSelected);
    on<QrScanned>(_onQrScanned);
    on<SmsParseRequested>(_onSmsParse);
    on<ManualEntrySubmitted>(_onManualEntry);
    on<CaptureConfirmed>(_onConfirmed);
    on<CaptureCancelled>((_, emit) => emit(CaptureIdle()));
  }

  Future<void> _onVoiceRecordingStopped(VoiceRecordingStopped event, Emitter<CaptureState> emit) async {
    emit(CaptureProcessing());
    try {
      List<int> bytes;
      if (kIsWeb) {
        final dio = Dio();
        final response = await dio.get<List<int>>(
          event.audioFilePath,
          options: Options(responseType: ResponseType.bytes),
        );
        bytes = response.data ?? [];
      } else {
        final file = File(event.audioFilePath);
        bytes = await file.readAsBytes();
      }

      final formData = FormData.fromMap({
        'audioFile': MultipartFile.fromBytes(
          bytes,
          filename: 'voice_capture.m4a',
          contentType: MediaType('audio', 'm4a'),
        ),
      });

      final response = await _apiClient.dio.post('/voice/capture', data: formData);

      emit(CapturePreview(_mapResponse(response.data, 'Voice')));
    } catch (e) {
      emit(CaptureError('Failed to process voice. Please try again.'));
    }
  }

  Future<void> _onOcrImageSelected(OcrImageSelected event, Emitter<CaptureState> emit) async {
    emit(CaptureProcessing());
    try {
      final bytes = await event.imageFile.readAsBytes();
      final formData = FormData.fromMap({
        'imageFile': MultipartFile.fromBytes(bytes, filename: 'receipt.jpg', contentType: MediaType('image', 'jpeg')),
      });

      final response = await _apiClient.dio.post('/ocr/receipt', data: formData);
      emit(CapturePreview(_mapResponse(response.data, 'Receipt OCR')));
    } catch (e) {
      emit(CaptureError('Failed to extract receipt data. Please try again.'));
    }
  }

  Future<void> _onQrScanned(QrScanned event, Emitter<CaptureState> emit) async {
    emit(CaptureProcessing());
    try {
      final response = await _apiClient.dio.post('/qr/parse', data: {'qrContent': event.qrContent});
      emit(CapturePreview(_mapResponse(response.data, 'QR Code')));
    } catch (e) {
      emit(CaptureError('Unrecognized QR code format. Please enter manually.'));
    }
  }

  Future<void> _onSmsParse(SmsParseRequested event, Emitter<CaptureState> emit) async {
    emit(CaptureProcessing());
    try {
      final response = await _apiClient.dio.post('/sms/parse', data: {
        'smsBody': event.smsBody,
        'sender': event.sender,
      });
      emit(CapturePreview(_mapResponse(response.data, 'SMS')));
    } catch (e) {
      emit(CaptureError('Could not parse SMS. Is this a bank transaction message?'));
    }
  }

  Future<void> _onManualEntry(ManualEntrySubmitted event, Emitter<CaptureState> emit) async {
    emit(CapturePreview(CaptureResult(
      merchant: event.merchant,
      amount: event.amount,
      currency: 'EGP',
      category: event.category ?? 'Other',
      categoryName: event.category ?? 'Other',
      confidenceScore: 1.0,
      captureSource: 'Manual',
    )));
  }

  Future<void> _onConfirmed(CaptureConfirmed event, Emitter<CaptureState> emit) async {
    emit(CaptureSuccess());
    await Future.delayed(const Duration(milliseconds: 300));
    emit(CaptureIdle());
  }

  CaptureResult _mapResponse(Map<String, dynamic> data, String source) {
    return CaptureResult(
      merchant: data['merchant'] ?? 'Unknown',
      amount: (data['amount'] as num).toDouble(),
      currency: data['currency'] ?? 'EGP',
      category: data['category'] ?? 'Other',
      categoryName: data['categoryName'] ?? 'Other',
      confidenceScore: (data['confidenceScore'] as num?)?.toDouble() ?? 0.9,
      captureSource: source,
    );
  }
}
