using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace TXABackupTool.Services;

/// <summary>
/// TXA VLOG Language Service - Bản quyền bảo mật cao cấp 2026
/// </summary>
public static class LanguageService
{
    #region Security Core
    // --- LÕI BẢO MẬT THƯƠNG HIỆU (BRANDED SECURITY CORE) ---
    // Sử dụng Hashing để tạo Key/IV từ Slogan thương hiệu (Sang và bảo mật hơn)
    private static readonly byte[] Key = SHA256.HashData(Encoding.UTF8.GetBytes("TXA_VLOG_PREMIUM_SECURITY_PROTECTION_2026"));
    private static readonly byte[] IV = MD5.HashData(Encoding.UTF8.GetBytes("TXA_VLOG_IDENTITY_IV_SIGNATURE"));
    #endregion

    #region Validation & Import Types
    public enum ValidationStatus { Success, Layer1Invalid, Layer2Invalid, AlreadyExists }
    public record LanguageValidationResult(ValidationStatus Status, string Message, string? LangName = null);
    #endregion

    #region Properties & State
    // State quản lý ngôn ngữ
    public static Dictionary<string, string> CurrentLanguage { get; private set; } = new();
    public static string CurrentLanguageName { get; private set; } = "Vietnamese";
    public static string CurrentLanguageDescription { get; private set; } = "Ngôn ngữ mặc định";

    /// <summary>
    /// Fired sau khi ngôn ngữ được nạp thành công — dùng để refresh tooltip, UI strings, etc.
    /// </summary>
    public static event Action? LanguageChanged;
    public static string? CurrentLanguagePath => _loadedFilePath;
    #endregion

    #region Default Dictionaries (Đầy đủ 100% Keys từ bản gốc)
    private static readonly Dictionary<string, string> DefaultDictVI = new()
    {
        { "txa_backup_title", "SAO LƯU DỮ LIỆU" },
        { "txa_restore_title", "KHÔI PHỤC DỮ LIỆU" },
        { "txa_choose_folder", "CHỌN THƯ MỤC" },
        { "txa_dest", "ĐÍCH ĐẾN" },
        { "txa_start", "BẮT ĐẦU" },
        { "txa_cancel", "HỦY BỎ" },
        { "txa_logs", "NHẬT KÝ HOẠT ĐỘNG" },
        { "txa_open_tm", "MỞ" },
        { "txa_nav_backup", "Sao lưu" },
        { "txa_nav_restore", "Khôi phục" },
        { "txa_nav_logs", "Nhật ký" },
        { "txa_nav_storage", "Lưu trữ" },
        { "txa_nav_settings", "Cài đặt" },
        { "txa_label_lang", "NGÔN NGỮ" },
        { "txa_btn_open_tm", "MỞ THƯ MỤC CÀI ĐẶT" },
        { "txa_coming_soon", "SẮP RA MẮT" },
        { "txa_coming_soon_desc", "Tính năng này đang được phát triển." },
        { "txa_restore_include_text", "Bao gồm văn bản" },
        { "txa_browse", "Duyệt" },
        { "txa_btn_clear_logs", "XÓA TẤT CẢ" },
        { "txa_btn_copy", "Sao chép" },
        { "txa_status_scanning", "Đang quét..." },
        { "txa_status_cancelled", "Đã hủy" },
        { "txa_log_scan", "[QUÉT] Đang quét: {0}" },
        { "txa_log_copy_ok", "[OK] Đã sao lưu {0}" },
        { "txa_info_data", "Tổng dữ liệu: {0}" },
        { "txa_info_copyright", "© {0} TXA VLOG — BẢN QUYỀN ĐÃ ĐƯỢC BẢO HỘ." },
        { "txa_restore_clipboard", "KHÔI PHỤC CLIPBOARD" },
        { "txa_log_restore_cb", "[PHỤC HỒI] Đang khôi phục dữ liệu từ thư mục..." },
        { "txa_log_restore_ok", "[OK] Đã khôi phục thành công!" },
        { "txa_restore_desc", "Hãy chọn thư mục chứa bản sao lưu (Backup) bạn đã tạo trước đó để tiến hành khôi phục." },
        { "txa_restore_path_label", "THƯ MỤC CHỨA BẢN BACKUP" },
        { "txa_restore_no_folder_err", "BẠN CHƯA CHỌN THƯ MỤC CHỨA BẢN BACKUP!" },
        { "txa_restore_invalid_err", "THƯ MỤC NÀY KHÔNG CHỨA BẢN SAO LƯU HỢP LỆ!" },
        { "txa_source_dir", "THƯ MỤC NGUỒN BACKUP" },
        { "txa_summary_error", "CHI TIẾT LỖI" },
        { "txa_logs_empty", "¯\\_(ツ)_/¯ Chưa có bản ghi nào ở mục này!" },
        { "txa_tooltip_add", "Thêm file/thư mục" },
        { "txa_tooltip_backup", "Tiến hành sao lưu" },
        { "txa_tooltip_change_dest", "Thay đổi thư mục sao lưu" },
        { "txa_tooltip_start", "Bắt đầu thực hiện" },
        { "txa_tooltip_browse", "Duyệt tìm thư mục" },
        { "txa_tooltip_start_backup", "Tiến hành sao lưu dữ liệu ngay" },
        { "txa_folder_game", "Game MiniWorld" },
        { "txa_folder_browser", "Trình duyệt Cốc Cốc" },
        { "txa_folder_downloads", "Thư mục tải xuống" },
        { "txa_folder_pictures", "Thư viện ảnh" },
        { "txa_folder_videos", "Thư viện video" },
        { "txa_folder_desktop", "Màn hình chính (Desktop)" },
        { "txa_folder_clipboard", "Lịch sử Clipboard" },
        { "txa_btn_add_folder", "+ Thêm thư mục khác..." },
        { "txa_btn_change", "Thay đổi" },
        { "txa_overall_progress", "TIẾN TRÌNH TỔNG QUÁT" },
        { "txa_btn_clear_category", "Xóa mục lọc này" },
        { "txa_category", "📁 PHÂN LOẠI:" },
        { "txa_auto_lang_status", "Phát hiện lỗi ngôn ngữ tự động: {0}" },
        { "txa_on", "BẬT" },
        { "txa_off", "TẮT" },
        { "txa_info_header", "--- CHI TIẾT SAO LƯU TXA ({0}) ---" },
        { "txa_info_user", "Người dùng: {0}" },
        { "txa_info_machine", "Tên máy: {0}" },
        { "txa_info_files", "Số mục sao lưu: {0}" },
        { "txa_log_finish", "[HOÀN TẤT] Sao lưu kết thúc lúc {0}" },
        { "txa_log_stats", "[STATS] Tổng dung lượng đã nén: {0}" },
        { "txa_log_start_backup", "[BẮT ĐẦU] Đang chuẩn bị tiến trình sao lưu..." },
        { "txa_status_copying", "Đang chép" },
        { "txa_status_completed", "Xong" },
        { "txa_status_new", "Mới" },
        { "txa_log_copy_err", "[LỖI] Không thể sao lưu {0}: {1}" },
        { "txa_log_error", "[LỖI HỆ THỐNG] {0}" },
        { "txa_log_cancel", "[HỦY] Người dùng đã dừng tiến trình." },
        { "txa_eta", "Thời gian còn lại: {0}" },
        { "txa_summary_title", "HOÀN TẤT SAO LƯU" },
        { "txa_summary_total_time", "🛡️ Tổng thời gian: {0}" },
        { "txa_summary_files", "📄 Files: {0} thành công, {1} thất bại" },
        { "txa_summary_folders", "📁 Thư mục: {0} thành công, {1} thất bại" },
        // Storage page
        { "txa_storage_subtitle", "Cấu hình không gian và quản lý tài nguyên dữ liệu an toàn." },
        { "txa_storage_refresh_tip", "Kiểm tra lại dung lượng ổ đĩa" },
        { "txa_storage_drive_stats", "Thống kê Ổ đĩa" },
        { "txa_storage_drive_usage", "Còn trống {0} ({1}% đã sử dụng)" },
        { "txa_storage_drive_full", "Ổ ĐĨA ĐÃ ĐẦY" },
        { "txa_storage_refresh", "🔄 Kiểm tra" },
        { "txa_storage_quick_actions", "THAO TÁC NHANH" },
        { "txa_storage_change_folder", "Thay đổi thư mục đích" },
        { "txa_storage_open_folder", "Mở thư mục đích" },
        { "txa_storage_backup_locations", "Điểm đến Sao lưu" },
        { "txa_storage_local_path", "Local Path" },
        { "txa_storage_cloud_sync", "Cloud Sync" },
        { "txa_storage_cloud_status", "Google Drive (Chưa kết nối)" },
        { "txa_storage_connected", "ĐÃ KẾT NỐI" },
        { "txa_status_processed", "● {0} mục" },
        { "txa_status_skipped", "▲ {0} bỏ qua" },
        { "txa_status_ready", "📦 Sẵn sàng" },
        { "txa_storage_not_connected", "CHƯA KẾT NỐI" },
        { "txa_storage_last_updated", "CẬP NHẬT LÚC: {0}" },
        { "txa_storage_settings_title", "Thiết lập Lưu trữ" },
        { "txa_storage_autoclean_title", "Tự động dọn dẹp log cũ" },
        { "txa_storage_autoclean_desc", "Xóa bản ghi nhật ký lớn hơn 30 ngày để tiết kiệm dung lượng." },
        { "txa_storage_compress_title", "Nén file backup (.zip)" },
        { "txa_storage_compress_desc", "Nén toàn bộ dữ liệu trước khi lưu trữ để giảm ~40% dung lượng." },
        { "txa_logs_no_data", "Không có dữ liệu bản ghi nào trong mục này." },
        { "txa_log_filter_tip", "Lọc nhật ký theo loại" },
        // Zipper Logs
        { "txa_log_zip_start", "[ZIP] Đang bắt đầu nén: {0}" },
        { "txa_log_zip_entry", "[ZIP] Đang nén: {0}" },
        { "txa_log_zip_shortcut", "[ZIP] Đã thêm Contact Shortcut" },
        { "txa_log_zip_ok", "[ZIP OK] Đã nén thành công: {0}" },
        { "txa_log_zip_error", "[ZIP LỖI] {0}: {1}" },
        { "txa_log_zip_cleaning", "[ZIP] Đang dọn dẹp thư mục gốc..." },
        { "txa_zip_password_label", "MẬT KHẨU ZIP: {0}" },
        { "txa_zip_password_input", "Mật khẩu nén (Không bắt buộc)" },
        { "txa_live_status", "LIVE REAL-TIME" },
        { "txa_modal_shortcut_title", "NHẬN DIỆN SHORTCUT" },
        { "txa_modal_shortcut_desc", "Ứng dụng phát hiện một số tệp shortcut (.lnk, .url). Bạn có muốn giữ lại tệp nào không? (Mặc định sẽ bỏ qua)" },
        { "txa_btn_continue_backup", "TIẾP TỤC BACKUP" },
        { "txa_modal_resume_title", "PHÁT HIỆN DỮ LIỆU CŨ" },
        { "txa_modal_resume_desc", "Đã có dữ liệu backup từ trước trong thư mục đích. Bạn có muốn tiếp tục (chỉ copy những tệp mới/thay đổi) hay ghi đè toàn bộ?" },
        { "txa_btn_resume", "TIẾP TỤC (RESUME)" },
        { "txa_btn_overwrite", "LÀM MỚI (OVERWRITE)" },
        { "txa_btn_ok", "ĐÃ HIỂU (OK)" },
        // Settings page
        { "txa_settings_subtitle", "Tùy chỉnh hành vi và giao diện của ứng dụng." },
        { "txa_settings_lang_section", "Ngôn ngữ" },
        { "txa_settings_lang_desc", "Chọn ngôn ngữ hiển thị cho toàn bộ giao diện." },
        { "txa_settings_behavior_section", "Hành vi Sao lưu" },
        { "txa_settings_skip_shortcuts_title", "Bỏ qua tệp shortcut" },
        { "txa_settings_skip_shortcuts_desc", "Tự động bỏ qua các file .lnk và .url trong quá trình sao lưu." },
        { "txa_settings_autolang_title", "Phát hiện lỗi ngôn ngữ tự động" },
        { "txa_settings_autolang_desc", "Tự động ghi log khi phát hiện key ngôn ngữ bị thiếu." },
        { "txa_settings_about_section", "Thông tin ứng dụng" },
        { "txa_settings_version", "Phiên bản 1.0.0 — Build 2026" },
        { "txa_settings_author", "Tác giả: TXA" },
        { "txa_settings_contact", "Liên hệ: fb.com/vlog.txa.2311" },
        { "txa_settings_topmost_title", "Ghim trên cùng (Always on top)" },
        { "txa_settings_topmost_desc", "Giữ cửa sổ ứng dụng luôn hiển thị phía trên các ứng dụng khác." },
        { "txa_settings_topmost_tip", "Bật để luôn thấy ứng dụng" },
        { "txa_settings_skip_shortcuts_tip", "Giảm dung lượng bằng cách bỏ qua shortcut" },
        { "txa_settings_autolang_tip", "Ghi log khi thiếu ngôn ngữ" },
        { "txa_settings_lang_tip", "Chọn ngôn ngữ bạn muốn" },
        { "txa_storage_change_folder_tip", "Chọn thư mục đích để lưu bản sao lưu" },
        { "txa_storage_edit_path_tip", "Thay đổi đường dẫn lưu trữ" },
        { "txa_storage_open_dest_tip", "Mở thư mục đích trong File Explorer" },
        { "txa_storage_autoclean_tip", "Tự động xóa log cũ hơn 30 ngày" },
        { "txa_storage_compress_tip", "Nén dữ liệu backup thành file .zip có mật khẩu" },
        { "txa_settings_open_dir_tip", "Mở thư mục cài đặt ứng dụng" },
        { "txa_log_copy_tip", "Sao chép nội dung log vào clipboard" },
        { "txa_log_clear_tip", "Xóa tất cả bản ghi nhật ký" },
        { "txa_log_clear_cat_tip", "Xóa các bản ghi thuộc mục lọc hiện tại" },
        { "txa_restore_browse_tip", "Duyệt tìm file .txab để khôi phục" },
        { "txa_restore_start_tip", "Bắt đầu khôi phục dữ liệu từ file backup" },
        { "txa_lang_vi_name", "Tiếng Việt" },
        // Restore ZIP
        { "txa_restore_zip_source", "CHỌN FILE BACKUP (.ZIP)" },
        { "txa_restore_zip_password_label", "MẬT KHẨU GIẢI NÉN" },
        { "txa_zip_decompressing", "[ZIP] Đang giải nén gói dữ liệu..." },
        { "txa_zip_decompress_ok", "[ZIP] Giải nén thành công." },
        { "txa_zip_decompress_failed", "[LỖI] Giải nén thất bại!" },
        { "txa_zip_password_wrong", "[LỖI] Sai mật khẩu! Vui lòng kiểm tra lại." },
        { "txa_zip_compressing", "[ZIP] Đang tiến hành nén toàn bộ dữ liệu..." },
        { "txa_zip_compressing_special", "[ZIP] Đang nén Clipboard & Thông tin..." },
        { "txa_dialog_choose_dest", "Chọn thư mục đích sao lưu" },
        { "txa_log_resume_mode", "Chế độ: Tiếp tục backup bản cũ (Skip file trùng)" },
        { "txa_storage_system_drive", "Ổ hệ thống ({0})" },
        { "txa_storage_data_partition", "Phân vùng dữ liệu ({0})" },
        { "txa_btn_modal_ok", "ĐÃ HIỂU (OK)" }
    };

    private static readonly Dictionary<string, string> DefaultDictEN = new()
    {
        { "txa_backup_title", "DATA BACKUP" },
        { "txa_restore_title", "DATA RESTORE" },
        { "txa_choose_folder", "CHOOSE FOLDER" },
        { "txa_dest", "DESTINATION" },
        { "txa_start", "START" },
        { "txa_cancel", "CANCEL" },
        { "txa_logs", "ACTIVITY LOGS" },
        { "txa_open_tm", "OPEN" },
        { "txa_nav_backup", "Backup" },
        { "txa_nav_restore", "Restore" },
        { "txa_nav_logs", "Logs" },
        { "txa_nav_storage", "Storage" },
        { "txa_nav_settings", "Settings" },
        { "txa_label_lang", "LANGUAGE" },
        { "txa_btn_open_tm", "OPEN INSTALL DIR" },
        { "txa_coming_soon", "COMING SOON" },
        { "txa_coming_soon_desc", "This feature is currently under development." },
        { "txa_restore_include_text", "Include text content" },
        { "txa_browse", "Browse" },
        { "txa_btn_clear_logs", "CLEAR ALL" },
        { "txa_btn_copy", "Copy" },
        { "txa_status_scanning", "Scanning..." },
        { "txa_status_cancelled", "Cancelled" },
        { "txa_log_scan", "[SCAN] Scanning: {0}" },
        { "txa_log_copy_ok", "[OK] Backed up {0}" },
        { "txa_info_data", "Total Data: {0}" },
        { "txa_info_copyright", "© {0} TXA VLOG — ALL RIGHTS RESERVED." },
        { "txa_restore_clipboard", "RESTORE CLIPBOARD" },
        { "txa_log_restore_cb", "[RESTORE] Restoring data from folder..." },
        { "txa_log_restore_ok", "[OK] Restore completed successfully!" },
        { "txa_restore_desc", "Please select the folder containing your previously created backup to begin restoration." },
        { "txa_restore_path_label", "BACKUP FOLDER SOURCE" },
        { "txa_restore_no_folder_err", "YOU HAVE NOT SELECTED A BACKUP FOLDER!" },
        { "txa_restore_invalid_err", "THIS FOLDER DOES NOT CONTAIN A VALID BACKUP!" },
        { "txa_source_dir", "BACKUP SOURCE DIRECTORY" },
        { "txa_summary_error", "ERROR DETAILS" },
        { "txa_logs_empty", "¯\\_(ツ)_/¯ No logs found in this category!" },
        { "txa_tooltip_add", "Add file/folder" },
        { "txa_tooltip_backup", "Start backup" },
        { "txa_tooltip_change_dest", "Change destination folder" },
        { "txa_tooltip_start", "Start the process" },
        { "txa_tooltip_browse", "Browse for folder" },
        { "txa_tooltip_start_backup", "Start data backup now" },
        { "txa_folder_game", "MiniWorld Gaming" },
        { "txa_folder_browser", "CocCoc Browser" },
        { "txa_folder_downloads", "Downloads Folder" },
        { "txa_folder_pictures", "Pictures Library" },
        { "txa_folder_videos", "Videos Library" },
        { "txa_folder_desktop", "Desktop Files" },
        { "txa_folder_clipboard", "Clipboard History" },
        { "txa_btn_add_folder", "+ Add other folder..." },
        { "txa_btn_change", "Change" },
        { "txa_overall_progress", "OVERALL PROGRESS" },
        { "txa_btn_clear_category", "Clear this filter" },
        { "txa_category", "📁 CATEGORY:" },
        { "txa_auto_lang_status", "Auto language error detection: {0}" },
        { "txa_on", "ON" },
        { "txa_off", "OFF" },
        { "txa_info_header", "--- TXA BACKUP DETAILS ({0}) ---" },
        { "txa_info_user", "User: {0}" },
        { "txa_info_machine", "Machine Name: {0}" },
        { "txa_info_files", "Items Backed Up: {0}" },
        { "txa_log_finish", "[FINISHED] Backup ended at {0}" },
        { "txa_log_stats", "[STATS] Total Compressed Size: {0}" },
        { "txa_log_start_backup", "[START] Preparing backup process..." },
        { "txa_status_copying", "Copying" },
        { "txa_status_completed", "Done" },
        { "txa_status_new", "New" },
        { "txa_log_copy_err", "[ERROR] Failed to backup {0}: {1}" },
        { "txa_log_error", "[SYSTEM ERROR] {0}" },
        { "txa_log_cancel", "[CANCEL] Process stopped by user." },
        { "txa_eta", "Time remaining: {0}" },
        { "txa_summary_title", "BACKUP COMPLETED" },
        { "txa_summary_total_time", "🛡️ Total time: {0}" },
        { "txa_summary_files", "📄 Files: {0} success, {1} failed" },
        { "txa_summary_folders", "📁 Folders: {0} backed up, {1} failed" },
        // Storage page
        { "txa_storage_subtitle", "Configure storage space and manage backup resources securely." },
        { "txa_storage_refresh_tip", "Refresh drive capacity" },
        { "txa_storage_drive_stats", "Drive Statistics" },
        { "txa_storage_drive_usage", "{0} free ({1}% USED)" },
        { "txa_storage_drive_full", "DRIVE FULL (100% USED)" },
        { "txa_storage_refresh", "🔄 Refresh" },
        { "txa_storage_quick_actions", "QUICK ACTIONS" },
        { "txa_storage_change_folder", "Change destination folder" },
        { "txa_storage_open_folder", "Open destination folder" },
        { "txa_storage_backup_locations", "Backup Destinations" },
        { "txa_storage_local_path", "Local Path" },
        { "txa_storage_cloud_sync", "Cloud Sync" },
        { "txa_storage_cloud_status", "Google Drive (Not connected)" },
        { "txa_storage_connected", "CONNECTED" },
        { "txa_status_processed", "● {0} items" },
        { "txa_status_skipped", "▲ {0} skipped" },
        { "txa_status_ready", "📦 Ready" },
        { "txa_storage_not_connected", "NOT CONNECTED" },
        { "txa_storage_last_updated", "LAST UPDATED: {0}" },
        { "txa_storage_settings_title", "Storage Settings" },
        { "txa_storage_autoclean_title", "Auto-clean old logs" },
        { "txa_storage_autoclean_desc", "Delete log records older than 30 days to save disk space." },
        { "txa_storage_compress_title", "Compress backup (.zip)" },
        { "txa_storage_compress_desc", "Compress all data before storing to reduce size by ~40%." },
        { "txa_logs_no_data", "No log records found in this category." },
        { "txa_log_filter_tip", "Filter logs by type" },
        // Zipper Logs
        { "txa_log_zip_start", "[ZIP] Starting compression: {0}" },
        { "txa_log_zip_entry", "[ZIP] Compressing: {0}" },
        { "txa_log_zip_shortcut", "[ZIP] Added Contact Shortcut" },
        { "txa_log_zip_ok", "[ZIP OK] Successfully compressed: {0}" },
        { "txa_log_zip_error", "[ZIP ERROR] {0}: {1}" },
        { "txa_log_zip_cleaning", "[ZIP] Cleaning up temporary files..." },
        { "txa_zip_password_label", "ZIP PASSWORD: {0}" },
        { "txa_zip_password_input", "Compression password (Optional)" },
        { "txa_live_status", "LIVE REAL-TIME" },
        { "txa_modal_shortcut_title", "SHORTCUT DETECTION" },
        { "txa_modal_shortcut_desc", "The application detected some shortcut files (.lnk, .url). Do you want to keep any of them? (Default is skip)" },
        { "txa_btn_continue_backup", "CONTINUE BACKUP" },
        { "txa_modal_resume_title", "EXISTING DATA DETECTED" },
        { "txa_modal_resume_desc", "Previous backup data exists in the destination. Do you want to resume (copy only new/changed files) or overwrite all?" },
        { "txa_btn_resume", "RESUME" },
        { "txa_btn_overwrite", "OVERWRITE" },
        { "txa_btn_ok", "UNDERSTOOD (OK)" },
        // Settings page
        { "txa_settings_subtitle", "Customize the application's behavior and appearance." },
        { "txa_settings_lang_section", "Language" },
        { "txa_settings_lang_desc", "Select the display language for the entire interface." },
        { "txa_settings_behavior_section", "Backup Behavior" },
        { "txa_settings_skip_shortcuts_title", "Skip shortcut files" },
        { "txa_settings_skip_shortcuts_desc", "Automatically skip .lnk and .url files during backup." },
        { "txa_settings_autolang_title", "Auto language error detection" },
        { "txa_settings_autolang_desc", "Automatically log when missing language keys are detected." },
        { "txa_settings_about_section", "About" },
        { "txa_settings_version", "Version 1.0.0 — Build 2026" },
        { "txa_settings_author", "Author: TXA" },
        { "txa_settings_contact", "Contact: fb.com/vlog.txa.2311" },
        { "txa_settings_topmost_title", "Always on top" },
        { "txa_settings_topmost_desc", "Keep the application window always on top of other applications." },
        { "txa_settings_topmost_tip", "Enable to keep app visible" },
        { "txa_settings_skip_shortcuts_tip", "Save space by skipping shortcuts" },
        { "txa_settings_autolang_tip", "Log missing language keys" },
        { "txa_settings_lang_tip", "Choose your language" },
        { "txa_lang_vi_name", "HOANG SA & TRUONG SA BELONG TO VIETNAM" },
        // Restore ZIP
        { "txa_restore_zip_source", "SELECT BACKUP FILE (.ZIP)" },
        { "txa_restore_zip_password_label", "UNZIP PASSWORD" },
        { "txa_zip_decompressing", "[ZIP] Decompressing backup bundle..." },
        { "txa_zip_decompress_ok", "[ZIP] Decompression successful." },
        { "txa_zip_decompress_failed", "[ERROR] Decompression failed!" },
        { "txa_zip_password_wrong", "[ERROR] Wrong password! Please check and try again." },
        { "txa_zip_compressing", "[ZIP] Compressing entire backup folder..." },
        { "txa_zip_compressing_special", "[ZIP] Compressing Clipboard & Info..." },
        { "txa_dialog_choose_dest", "Choose backup destination folder" },
        { "txa_log_resume_mode", "Mode: Resume previous backup (Skip duplicate files)" },
        { "txa_storage_system_drive", "System Drive ({0})" },
        { "txa_storage_data_partition", "Data Partition ({0})" },
        { "txa_btn_modal_ok", "UNDERSTOOD (OK)" }
    };
    #endregion

    #region Language Discovery & State
    private static string? _loadedFilePath;
    public static List<LanguageItem> AvailableLanguages { get; private set; } = new();

    public class LanguageItem 
    { 
        public string Name { get; set; } = ""; 
        public string Path { get; set; } = ""; 
        public string Description { get; set; } = "";
    }

    public static List<string> GetAvailableLanguages()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "lang");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        string viFile = Path.Combine(path, "vietnamese.txaf");
        string enFile = Path.Combine(path, "english.txaf");

        if (!File.Exists(viFile)) InitializeDefaultFile(viFile);
        if (!File.Exists(enFile)) InitializeDefaultFile(enFile);

        var files = Directory.GetFiles(path, "*.txaf").ToList();
        
        // Cập nhật danh sách cache để các ViewModel dùng chung, tránh scan nhiểu lần
        var newList = new List<LanguageItem>();
        foreach(var f in files)
        {
            // Tạm thời load để lấy tên (chỉ chạy 1 lần khi startup)
            LoadLanguage(f, true); 
            newList.Add(new LanguageItem { 
                Name = CurrentLanguageName, 
                Path = f, 
                Description = CurrentLanguageDescription 
            });
        }
        AvailableLanguages = newList;
        
        return files;
    }
    #endregion

    #region Language Loading
    /// <summary>
    /// Nạp ngôn ngữ và tự động đọc metadata (tname, tdesc) từ file đã mã hóa
    /// </summary>
    public static void LoadLanguage(string filePath, bool force = false)
    {
        // Chặn đệ quy/redundant load
        if (!force && _loadedFilePath == filePath) return;

        string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TXABackupTool", "Logs", "startup_debug.log");
        void LogB(string msg) { try { File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss.fff}] [LANG] {msg}\n"); } catch { } }

        try
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string json = Decrypt(encryptedData);
                
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("Tlang", out var tlangElement))
                {
                    CurrentLanguage = JsonSerializer.Deserialize<Dictionary<string, string>>(tlangElement.GetRawText()) ?? new();
                    
                    if (root.TryGetProperty("tname", out var tname)) 
                        CurrentLanguageName = tname.GetString() ?? "Unknown";

                    if (root.TryGetProperty("tdesc", out var tdesc)) 
                        CurrentLanguageDescription = tdesc.GetString() ?? "";
                    
                    _loadedFilePath = filePath;
                    LanguageChanged?.Invoke();
                    return;
                }
            }

            InitializeDefaultFile(filePath);
            _loadedFilePath = filePath;
        }
        catch (Exception ex)
        { 
            LogB($"LOAD ERROR: {ex.Message}");
            CurrentLanguage = new Dictionary<string, string>(DefaultDictVI);
            CurrentLanguageName = "Tiếng Việt (Lỗi)";
        }
    }

    /// <summary>
    /// Nhập ngôn ngữ mới với 2 tầng kiểm tra bảo mật (TXA IMPORT ENGINE)
    /// </summary>
    public static LanguageValidationResult ImportLanguage(string sourceFilePath)
    {
        try
        {
            if (!File.Exists(sourceFilePath)) 
                return new LanguageValidationResult(ValidationStatus.Layer1Invalid, "Selected file does not exist!");

            // --- LAYER 1: ENCRYPTION INTEGRITY ---
            byte[] encryptedData = File.ReadAllBytes(sourceFilePath);
            string json;
            try
            {
                json = Decrypt(encryptedData);
                if (string.IsNullOrEmpty(json) || json == "{}")
                    throw new Exception("Decryption failure.");
            }
            catch
            {
                return new LanguageValidationResult(ValidationStatus.Layer1Invalid, "Invalid file format or corrupted encryption!");
            }

            // --- LAYER 2: SCHEMA VALIDATION ---
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("Tlang", out _) || !root.TryGetProperty("tname", out var tname))
                {
                    return new LanguageValidationResult(ValidationStatus.Layer2Invalid, "Invalid language data structure (Header missing)!");
                }

                string langName = tname.GetString() ?? "Unknown";
                string fileName = Path.GetFileName(sourceFilePath);
                string destPath = Path.Combine(AppContext.BaseDirectory, "lang", fileName);

                // Check existence
                if (File.Exists(destPath))
                {
                    return new LanguageValidationResult(ValidationStatus.AlreadyExists, $"Language pack '{langName}' is already installed!", langName);
                }

                // Copy
                string langDir = Path.Combine(AppContext.BaseDirectory, "lang");
                if (!Directory.Exists(langDir)) Directory.CreateDirectory(langDir);
                
                File.Copy(sourceFilePath, destPath, true);
                
                // Refresh list
                GetAvailableLanguages();
                
                return new LanguageValidationResult(ValidationStatus.Success, $"Successfully imported language: {langName}", langName);
            }
            catch (JsonException)
            {
                return new LanguageValidationResult(ValidationStatus.Layer2Invalid, "Data corrupted: JSON syntax error after decryption!");
            }
        }
        catch (Exception ex)
        {
            return new LanguageValidationResult(ValidationStatus.Layer1Invalid, $"System error during import: {ex.Message}");
        }
    }

    private static void InitializeDefaultFile(string filePath)
    {
        bool isEn = filePath.ToLower().Contains("english");
        var dict = isEn ? DefaultDictEN : DefaultDictVI;
        
        var schema = new 
        { 
            tname = isEn ? "English" : "Tiếng Việt", 
            tdesc = isEn ? "Official English Language Pack" : "Ngôn ngữ hệ thống mặc định", 
            Tlang = dict 
        };

        string? dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
        // Lưu file mã hóa với cấu trúc mới
        string jsonContent = JsonSerializer.Serialize(schema);
        File.WriteAllBytes(filePath, Encrypt(jsonContent));

        // Cập nhật State
        CurrentLanguage = new Dictionary<string, string>(dict);
        CurrentLanguageName = schema.tname;
        CurrentLanguageDescription = schema.tdesc;
    }
    #endregion

    #region Data Handling
    public static string Get(string key, params object[] args)
    {
        if (!CurrentLanguage.TryGetValue(key, out var val))
        {
            if (!DefaultDictVI.TryGetValue(key, out val))
            {
                // Key hoàn toàn không tồn tại trong hệ thống
                TXALogger.Log(LogType.Localization, $"Missing key: {key} in {CurrentLanguageName}");
            }
        }

        if (string.IsNullOrEmpty(val)) return $"${{{key}}}";

        try { return string.Format(val, args); }
        catch { return val; }
    }
    #endregion

    #region LÕI AES CAO CẤP (TXA ENGINE)

    public static byte[] Encrypt(string text)
    {
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        return encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
    }

    private static string Decrypt(byte[] encrypted)
    {
        try
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            byte[] output = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            return Encoding.UTF8.GetString(output);
        }
        catch { return "{}"; }
    }
    #endregion
}