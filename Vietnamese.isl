; TXA VIETNAMESE LANGUAGE FILE FOR INNO SETUP 6.5.0+ 
; Updated placeholders to [name], [name/ver], etc. for Inno Setup 6+ compatibility.
; (c) 2026 TXA VLOG

[LangOptions]
LanguageName=Vietnamese
LanguageID=$042a
LanguageCodePage=1258

[Messages]
; --- XÁC NHẬN ---
ConfirmDeleteSharedFileTitle=Xác nhận xóa tệp chia sẻ
ConfirmDeleteSharedFile2=Tệp chia sẻ sau đây không còn được sử dụng bởi bất kỳ chương trình nào. Bạn có muốn xóa nó không?%n%nNếu bất kỳ chương trình nào khác vẫn sử dụng tệp này và tệp bị xóa, chương trình đó có thể không hoạt động chính xác. Nếu bạn không chắc chắn, hãy chọn Không. Xóa tệp có thể làm hỏng hệ thống của bạn.
WizardSelectDir=Chọn vị trí cài đặt
WizardSelectProgramGroup=Chọn thư mục Menu Start
WizardSelectComponents=Chọn thành phần
WizardSelectTasks=Chọn tác vụ bổ sung
WizardPreparing=Đang chuẩn bị cài đặt
WizardInstalling=Đang cài đặt
WizardReady=Sẵn sàng cài đặt
WizardUserInfo=Thông tin người dùng
WizardLicense=Thỏa thuận bản quyền
WizardPassword=Mật khẩu
WizardInfoBefore=Thông tin trước khi cài đặt
WizardInfoAfter=Thông tin sau khi cài đặt
WizardUninstalling=Trạng thái gỡ cài đặt

; --- NÚT BẤM ---
ButtonBack=< &Quay lại
ButtonNext=&Tiếp tục >
ButtonInstall=&Cài đặt
ButtonOK=Đồng ý
ButtonCancel=Hủy bỏ
ButtonYes=&Có
ButtonNo=&Không
ButtonFinish=&Kết thúc
ButtonBrowse=&Duyệt...
ButtonWizardBrowse=Duyê&t...
ButtonNewFolder=&Tạo thư mục mới
ButtonNoToAll=Không cho tấ&t cả
ButtonYesToAll=&Có cho tất cả
ButtonStopDownload=Dừng tải xuống
ButtonStopExtraction=Dừng giải nén

; --- CHÀO MỪNG ---
WelcomeLabel1=Chào mừng bạn đến với trình cài đặt [name]
WelcomeLabel2=Trình cài đặt sẽ cài đặt [name/ver] vào máy máy tính của bạn.%n%nBạn nên đóng tất cả các ứng dụng khác trước khi tiếp tục.
SetupAppTitle=Cài đặt ứng dụng
SetupWindowTitle=Cài đặt - %1

; --- VỊ TRÍ CÀI ĐẶT ---
SelectDirDesc=Bạn muốn cài đặt [name] vào thư mục nào?
SelectDirLabel3=Trình cài đặt sẽ cài đặt [name] vào thư mục sau.
SelectDirBrowseLabel=Để tiếp tục, hãy nhấn Tiếp tục. Nếu bạn muốn chọn thư mục khác, hãy nhấn Duyệt.
SelectDirectoryLabel=Chọn thư mục đích để cài đặt ứng dụng.
DiskSpaceMBLabel=Chương trình này cần ít nhất [mb] MB dung lượng trống trên ổ đĩa.
DiskSpaceGBLabel=Chương trình này cần ít nhất [gb] GB dung lượng trống trên ổ đĩa.
DiskSpaceWarningTitle=Không đủ dung lượng ổ đĩa
DiskSpaceWarning=Trình cài đặt cần ít nhất %1 KB dung lượng trống để cài đặt, nhưng ổ đĩa đã chọn chỉ có %2 KB khả dụng.%n%nBạn vẫn muốn tiếp tục chứ?

; --- THÀNH PHẦN ---
SelectComponentsDesc=Thành phần nào nên được cài đặt?
SelectComponentsLabel2=Chọn các thành phần bạn muốn cài đặt; bỏ chọn các thành phần bạn không muốn cài đặt. Nhấn Tiếp tục khi bạn đã sẵn sàng.
FullInstallation=Cài đặt đầy đủ
CompactInstallation=Cài đặt tối giản
CustomInstallation=Cài đặt tùy chọn
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Các thành phần đã chọn cần ít nhất [mb] MB dung lượng ổ đĩa.
ComponentsDiskSpaceGBLabel=Các thành phần đã chọn cần ít nhất [gb] GB dung lượng ổ đĩa.

; --- MENU START ---
SelectStartMenuFolderDesc=Trình cài đặt nên đặt các biểu tượng tắt ở đâu?
SelectStartMenuFolderLabel3=Trình cài đặt sẽ tạo các biểu tượng tắt của chương trình trong thư mục Menu Start sau đây.
SelectStartMenuFolderBrowseLabel=Để tiếp tục, hãy nhấn Tiếp tục. Nếu bạn muốn chọn thư mục khác, hãy nhấn Duyệt.
NoProgramGroupCheck2=Không tạo thư mục Menu Start
MustEnterGroupName=Bạn phải nhập tên thư mục.
GroupNameTooLong=Tên thư mục hoặc đường dẫn quá dài.
InvalidGroupName=Tên thư mục không hợp lệ.
BadGroupName=Tên thư mục không được chứa bất kỳ ký tự nào sau đây:%n%1

; --- TÁC VỤ BỔ SUNG ---
SelectTasksDesc=Các tác vụ bổ sung nào nên được thực hiện?
SelectTasksLabel2=Chọn các tác vụ bổ sung mà trình cài đặt nên thực hiện trong khi cài đặt [name], sau đó nhấn Tiếp tục.

; --- SẴN SÀNG ---
ReadyMemoUserInfo=Thông tin người dùng:
ReadyMemoDir=Vị trí cài đặt:
ReadyMemoType=Loại cài đặt:
ReadyMemoComponents=Thành phần đã chọn:
ReadyMemoGroup=Thư mục Menu Start:
ReadyMemoTasks=Tác vụ bổ sung:
ReadyLabel1=Trình cài đặt đã sẵn sàng bắt đầu cài đặt [name] trên máy tính của bạn.
ReadyLabel2a=Nhấn Cài đặt để tiếp tục, hoặc nhấn Quay lại nếu bạn muốn xem lại hay thay đổi thiết lập.
ReadyLabel2b=Nhấn Cài đặt để tiếp tục cài đặt.

; --- TRẠNG THÁI ---
InstallingLabel=Vui lòng đợi trong khi trình cài đặt cài [name] vào máy tính của bạn.
StatusCreateDirs=Đang tạo thư mục...
StatusExtractFiles=Đang giải nén tệp...
StatusCreateIcons=Đang tạo biểu tượng tắt...
StatusCreateIniEntries=Đang tạo mục INI...
StatusCreateRegistryEntries=Đang tạo mục Registry...
StatusRegisterFiles=Đang đăng ký tệp...
StatusSavingUninstall=Đang lưu thông tin gỡ cài đặt...
StatusRunProgram=Đang hoàn tất cài đặt...
StatusClosingApplications=Đang đóng các ứng dụng...
StatusRestartingApplications=Đang khởi động lại các ứng dụng...
StatusRollback=Đang hoàn tác các thay đổi...
StatusDownloadFiles=Đang tải xuống các tệp bổ sung...
StatusUninstalling=Đang gỡ cài đặt...

; --- HOÀN TẤT ---
FinishedHeadingLabel=Hoàn tất cài đặt [name]
FinishedLabelNoIcons=[name] đã được cài đặt vào máy tính của bạn.
FinishedLabel=[name] đã được cài đặt vào máy tính của bạn. Ứng dụng có thể được khởi chạy bằng cách chọn các biểu tượng tắt đã được tạo.
ClickFinish=Nhấn Kết thúc để thoát khỏi trình cài đặt.
FinishedRestartLabel=Cần khởi động lại máy tính
FinishedRestartMessage=Để hoàn tất quá trình cài đặt [name], bạn phải khởi động lại máy tính. Bạn có muốn khởi động lại ngay bây giờ không?
ShowReadmeCheck=Xem tệp Readme

; --- GỠ CÀI ĐẶT ---
ConfirmUninstall=Bạn có chắc chắn muốn gỡ bỏ hoàn toàn %1 và tất cả các thành phần của nó không?
UninstallStatusLabel=Vui lòng đợi trong khi %1 được gỡ bỏ khỏi máy tính của bạn.
UninstalledAll=%1 đã được gỡ bỏ thành công khỏi máy tính của bạn.
UninstalledMost=Quá trình gỡ bỏ %1 đã hoàn tất.%n%nMột số thành phần không thể gỡ bỏ được. Bạn có thể xóa chúng thủ công.
UninstalledAndNeedsRestart=Để hoàn tất việc gỡ bỏ %1, bạn phải khởi động lại máy tính.%n%nBạn có muốn khởi động lại ngay bây giờ không?
UninstallDisplayNameMarkAllUsers=%1 (Tất cả người dùng)
UninstallDisplayNameMarkCurrentUser=%1 (Người dùng hiện tại)
UninstallAppTitle=Gỡ cài đặt ứng dụng
UninstallAppFullTitle=Gỡ cài đặt %1
UninstallDataCorrupted=Dữ liệu gỡ cài đặt bị hỏng. Không thể gỡ bỏ ứng dụng.
UninstallNotFound=Không tìm thấy tệp thông tin gỡ cài đặt.
UninstallOpenError=Không thể mở tệp thông tin gỡ cài đặt.

; --- LỖI ---
ErrorTitle=Lỗi
ErrorCreatingDir=Trình cài đặt không thể tạo thư mục "%1"
ErrorTooManyFilesInDir=Không thể tạo tệp trong thư mục "%1" vì nó chứa quá nhiều tệp
ErrorReadingSource=Đã xảy ra lỗi khi cố gắng đọc tệp nguồn:%n%1
ErrorCopying=Đã xảy ra lỗi khi cố gắng sao chép một tệp:%n%1
ErrorExtracting=Đã xảy ra lỗi khi cố gắng giải nén một tệp:%n%1
ErrorRegCreateKey=Lỗi khi tạo biểu mẫu Registry:%n%1\%2
ErrorRegWriteKey=Lỗi khi ghi vào biểu mẫu Registry:%n%1\%2
ErrorExecutingProgram=Không thể thực thi tệp:%n%1
ErrorFileSize=Kích thước tệp không khớp.
ErrorFunctionFailed=Hàm gọi bị lỗi (Mã: %1).
ErrorFunctionFailedNoCode=Hàm gọi bị lỗi.
ErrorFunctionFailedWithMessage=Hàm gọi bị lỗi (Mã: %1).%n%2
ErrorInternal2=Lỗi nội bộ: %1
ErrorOpeningReadme=Không thể mở tệp Readme.
ErrorProgress=Đã xảy ra lỗi trong quá trình xử lý.
ErrorRegisterServer=Không thể đăng ký tệp DLL/OCX: %1
ErrorRegisterTypeLib=Không thể đăng ký thư viện loại: %1
ErrorRegOpenKey=Không thể mở khóa Registry: %1
ErrorRegSvr32Failed=Lệnh RegSvr32 thất bại với mã thoát %1
ErrorRenamingTemp=Lỗi khi đổi tên tệp tạm thời.
ErrorReplacingExistingFile=Lỗi khi thay thế tệp hiện có.
ErrorRestartingComputer=Không thể khởi động lại máy tính tự động. Vui lòng thực hiện thủ công.
ErrorRestartReplace=Lỗi khi thay thế tệp sau khi khởi động lại.
ErrorDownloadFailed=Tải xuống thất bại: %1 %2
ErrorDownloading=Đã xảy ra lỗi khi tải xuống %1
ErrorDownloadSizeFailed=Không thể lấy kích thước tải xuống: %1 %2
ErrorDownloadAborted=Tải xuống bị hủy.
ErrorExtractionAborted=Giải nén bị hủy.
ErrorExtractionFailed=Giải nén thất bại: %1

; --- CẢNH BÁO ---
BadDirName32=Tên thư mục không được chứa bất kỳ ký tự nào sau đây:%n%1
InvalidDirName=Tên thư mục không hợp lệ.
InvalidPath=Đường dẫn không hợp lệ.
InvalidDrive=Ổ đĩa không hợp lệ.
DirNameTooLong=Tên thư mục hoặc đường dẫn quá dài.
DirExistsTitle=Thư mục đã tồn tại
DirExists=Thư mục:%n%n%1%n%nđã tồn tại. Bạn vẫn muốn cài đặt vào thư mục này chứ?
DirDoesntExistTitle=Thư mục không tồn tại
DirDoesntExist=Thư mục:%n%n%1%n%nkhông tồn tại. Bạn có muốn tạo thư mục này không?
ExistingFileNewerSelectAction=Chọn hành động:
ExistingFileNewer2=Tệp hiện có mới hơn tệp sắp được cài đặt.
ExistingFileNewerOverwriteExisting=Ghi đè lên tệp mới hơn (Không khuyến nghị)
ExistingFileNewerKeepExisting=Giữ lại tệp hiện có (Khuyến nghị)
ExistingFileNewerOverwriteOrKeepAll=Tiếp tục áp dụng cho tất cả các tệp khác
ExistingFileReadOnly2=Không thể thay thế tệp vì nó được để ở chế độ Chỉ đọc.
ExistingFileReadOnlyRetry=Tắt thuộc tính Chỉ đọc và thử lại
ExistingFileReadOnlyKeepExisting=Giữ lại tệp hiện có
FileAbortRetryIgnoreSkipNotRecommended=Bỏ qua tệp này (Không khuyến nghị)
FileAbortRetryIgnoreIgnoreNotRecommended=Bỏ qua lỗi và tiếp tục (Không khuyến nghị)
FileExistsSelectAction=Chọn hành động:
FileExists2=Tệp đã tồn tại.
FileExistsOverwriteExisting=Ghi đè lên tệp hiện có
FileExistsKeepExisting=Giữ lại tệp hiện có
FileExistsOverwriteOrKeepAll=Tiếp tục áp dụng cho tất cả các tệp khác
FileNotInDir2=Tệp "%1" không tồn tại trong "%2".
ExitSetupTitle=Thoát cài đặt
ExitSetupMessage=Cài đặt chưa hoàn tất. Nếu bạn thoát ngay bây giờ, chương trình sẽ không được cài đặt đầy đủ.%n%nBạn có chắc chắn muốn thoát không?

; --- CỦA HỆ THỐNG ---
AbortRetryIgnoreCancel=Hủy bỏ
AbortRetryIgnoreSelectAction=Chọn hành động
AbortRetryIgnoreIgnore=Bỏ qua
AbortRetryIgnoreRetry=Thử lại
AdminPrivilegesRequired=Bạn phải đăng nhập với quyền quản trị viên để cài đặt chương trình này.
ApplicationsFound=Các ứng dụng sau đang sử dụng các tệp cần được cập nhật. Bạn nên để trình cài đặt tự động đóng các ứng dụng này.
ApplicationsFound2=Các ứng dụng sau đang sử dụng các tệp cần được cập nhật. Bạn nên để trình cài đặt tự động đóng các ứng dụng này. Sau khi cài đặt hoàn tất, trình cài đặt sẽ cố gắng khởi động lại các ứng dụng.
ArchiveIncorrectPassword=Mật khẩu không chính xác.
ArchiveIsCorrupted=Tệp lưu trữ bị hỏng.
ArchiveUnsupportedFormat=Định dạng lưu trữ không được hỗ trợ.
BeveledLabel=TXA VLOG
BrowseDialogLabel=Duyệt tìm thư mục trong danh sách dưới đây, sau đó nhấn Đồng ý.
BrowseDialogTitle=Duyệt tìm thư mục
CannotContinue=Không thể tiếp tục. Vui lòng khởi động lại cài đặt.
CannotInstallToNetworkDrive=Trình cài đặt không thể cài đặt vào ổ đĩa mạng.
CannotInstallToUNCPath=Trình cài đặt không thể cài đặt vào đường dẫn UNC.
ChangeDiskTitle=Thay đĩa
ClickNext=Nhấn Tiếp tục để tiếp tục, hoặc Hủy bỏ để thoát.
CloseApplications=Đóng các ứng dụng tự động
DontCloseApplications=Không đóng các ứng dụng
DownloadingLabel2=Đang tải xuống %1...
InfoAfterClickLabel=Để tiếp tục, hãy nhấn Tiếp tục.
InfoAfterLabel=Vui lòng đọc các thông tin quan trọng sau:
InfoBeforeClickLabel=Để tiếp tục, hãy nhấn Tiếp tục.
InfoBeforeLabel=Vui lòng đọc thông tin quan trọng sau trước khi tiếp tục:
InformationTitle=Thông tin
InvalidParameter=Tham số dòng lệnh không hợp lệ.
LastErrorMessage=%1.%n%nLỗi %2: %3
LdrCannotCreateTemp=Không thể tạo tệp tạm thời. Quá trình cài đặt bị hủy.
LdrCannotExecTemp=Không thể thực thi tệp trong thư mục tạm thời. Quá trình cài đặt bị hủy.
LicenseAccepted=Tôi đồng ý với các điều khoản
LicenseLabel=Vui lòng đọc Thỏa thuận bản quyền sau.
LicenseLabel3=Vui lòng đọc Thỏa thuận bản quyền sau. Bạn phải chấp nhận các điều khoản trước khi tiếp tục.
LicenseNotAccepted=Tôi không đồng ý
MustEnterGroupName=Bạn phải nhập tên thư mục Menu Start.
NewFolderName=Thư mục mới
NoRadio=Không
NotOnThisPlatform=Chương trình này không thể chạy trên hệ điều hành này.
NoUninstallWarning=Trình cài đặt phát hiện chương trình này đã được cài đặt. Tiếp tục sẽ ghi đè lên các tệp hiện có.
NoUninstallWarningTitle=Cảnh báo cài đặt lại
OnlyAdminCanUninstall=Chỉ người dùng có quyền quản trị mới có thể gỡ cài đặt chương trình này.
OnlyOnTheseArchitectures=Chương trình này chỉ có thể được cài đặt trên các phiên bản của Windows hỗ trợ các kiến trúc vi xử lý sau:%n%1
OnlyOnThisPlatform=Chương trình này chỉ có thể chạy trên %1.
PasswordEditLabel=&Mật khẩu:
PasswordLabel1=Vui lòng nhập mật khẩu.
PasswordLabel3=Vui lòng cung cấp mật khẩu, sau đó nhấn Tiếp tục để tiến hành. Mật khẩu nhạy cảm với chữ hoa và chữ thường.
PathLabel=&Đường dẫn:
PowerUserPrivilegesRequired=Bạn phải đăng nhập với quyền Quản trị viên hoặc quyền Người dùng nâng cao để cài đặt chương trình này.
PrepareToInstallNeedsRestart=Trình cài đặt phải khởi động lại máy tính. Sau khi khởi động lại, hãy chạy lại trình cài đặt để hoàn tất cài đặt [name].
PreparingDesc=Trình cài đặt đang chuẩn bị cài đặt [name] trên máy tính của bạn.
PreviousInstallNotCompleted=Quá trình cài đặt/gỡ bỏ trước đó chưa hoàn thành. Bạn cần khởi động lại máy tính để hoàn tất.
PrivilegesRequiredOverrideTitle=Chọn chế độ cài đặt
PrivilegesRequiredOverrideInstruction=Bạn muốn cài đặt [name] cho ai?
PrivilegesRequiredOverrideText1=[name] có thể được cài đặt cho tất cả người dùng (yêu cầu quyền quản trị), hoặc chỉ cho riêng bạn.
PrivilegesRequiredOverrideText2=[name] có thể được cài đặt chỉ cho riêng bạn, hoặc cho tất cả người dùng (yêu cầu quyền quản trị).
PrivilegesRequiredOverrideAllUsers=Cài đặt cho &tất cả người dùng
PrivilegesRequiredOverrideAllUsersRecommended=Cài đặt cho &tất cả người dùng (khuyến nghị)
PrivilegesRequiredOverrideCurrentUser=Cài đặt chỉ cho &tôi
PrivilegesRequiredOverrideCurrentUserRecommended=Cài đặt chỉ cho &tôi (khuyến nghị)
RetryCancelCancel=Hủy bỏ
RetryCancelRetry=Thử lại
RetryCancelSelectAction=Chọn hành động:
RunEntryExec=%1 thực thi
RunEntryShellExec=%1 mở
SelectLanguageLabel=Chọn ngôn ngữ để sử dụng trong quá trình cài đặt:
SelectLanguageTitle=Chọn ngôn ngữ cài đặt
SetupAborted=Cài đặt đã bị hủy. Chương trình chưa được cài đặt.
SetupAlreadyRunning=Cài đặt đã đang chạy.
SetupAppRunningError=Trình cài đặt phát hiện %1 đang chạy.%n%nVui lòng đóng tất cả các cửa sổ của chương trình, sau đó nhấn Đồng ý để tiếp tục, hoặc nhấn Hủy bỏ để thoát.
SetupFileCorrupt=Các tệp cài đặt bị hỏng. Vui lòng tải lại bộ cài.
SetupFileCorruptOrWrongVer=Các tệp cài đặt bị hỏng hoặc không tương thích với phiên bản này.
SetupFileMissing=Thiếu tệp "%1" trong thư mục cài đặt.
SetupLdrStartupMessage=Chào mừng bạn đến với trình cài đặt [name]. Bạn có muốn tiếp tục không?
SharedFileNameLabel=Tên tệp:
SharedFileLocationLabel=Vị trí:
ShutdownBlockReasonInstallingApp=Đang cài đặt [name].
ShutdownBlockReasonUninstallingApp=Đang gỡ cài đặt [name].
SourceDoesntExist=Tệp nguồn "%1" không tồn tại.
SourceIsCorrupted=Tệp nguồn bị hỏng.
SourceVerificationFailed=Xác minh tệp nguồn thất bại: %1
StopDownload=Dừng tải xuống
StopExtraction=Dừng giải nén
UninstallNotFound=Tệp nhật ký gỡ cài đặt không tồn tại. Không thể gỡ bỏ chương trình.
UninstallOnlyOnWin64=Chương trình này chỉ có thể được gỡ cài đặt trên Windows 64-bit.
UninstallUnknownEntry=Đã xảy ra lỗi khi cố gắng gỡ bỏ mục không xác định trong registry (%1)
UninstallUnsupportedVer=Tệp nhật ký gỡ cài đặt được tạo bởi phiên bản khác của trình cài đặt. Không thể thực hiện gỡ bỏ.
UserInfoDesc=Vui lòng nhập thông tin của bạn.
UserInfoName=&Tên người dùng:
UserInfoNameRequired=Bạn phải nhập tên người dùng.
UserInfoOrg=&Tổ chức:
UserInfoSerial=&Số Sê-ri:
VerificationFileHashIncorrect=Mã băm của tệp không khớp.
VerificationFileNameIncorrect=Tên tệp không chính xác.
VerificationFileSizeIncorrect=Kích thước tệp không chính xác.
VerificationFileTagIncorrect=Nhãn tệp không chính xác.
VerificationKeyNotFound=Không tìm thấy khóa xác minh.
VerificationSignatureDoesntExist=Tệp không có chữ ký.
VerificationSignatureInvalid=Chữ ký số không hợp lệ.
WindowsServicePackRequired=Chương trình này yêu cầu %1 Service Pack %2 hoặc mới hơn.
WindowsVersionNotSupported=Chương trình này không hỗ trợ phiên bản Windows đang chạy trên máy tính của bạn.
WinVersionTooHighError=Chương trình này không thể cài đặt trên %1 phiên bản %2 hoặc mới hơn.
WinVersionTooLowError=Chương trình này yêu cầu %1 phiên bản %2 hoặc mới hơn.
YesRadio=Có

[CustomMessages]
NameAndVersion=%1 phiên bản %2
AdditionalIcons=Biểu tượng bổ sung:
CreateDesktopIcon=Tạo biểu tượng ngoài &Desktop
CreateQuickLaunchIcon=Tạo biểu tượng Quick Launch
ProgramOnTheWeb=%1 trên trang web
UninstallProgram=Gỡ bỏ %1
LaunchProgram=Khởi chạy %1
AssocFileExtension=Liên kết %1 với đuôi tệp %2
AssocingFileExtension=Đang liên kết %1 với đuôi tệp %2...
AutoStartProgramGroupDescription=Khởi động cùng Windows:
AutoStartProgram=Tự động khởi chạy %1 khi máy tính khởi động
AddonHostProgramNotFound=Không tìm thấy chương trình lưu trữ của tiện ích mở rộng.%n%nBạn có muốn tìm nó thủ công không?
