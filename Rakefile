require "bundler/setup"
require "albacore"

EXECUTABLE = File.expand_path("../Foreman/bin/Release/Foreman.exe", __FILE__)

desc "Build"
msbuild :build do |msb|
  msb.properties :configuration => :Release
  msb.targets :Clean, :Build
  msb.solution = "Foreman.sln"
end

task :default => :build

## dist

require "erb"
require "fileutils"
require "tmpdir"

def source_files
  Dir[File.expand_path("../Foreman/**/*.cs", __FILE__)]
end

def clean(file)
  rm file if File.exists?(file)
end

def mkchdir(dir)
  FileUtils.mkdir_p(dir)
  Dir.chdir(dir) do |dir|
    yield(File.expand_path(dir))
  end
end

def pkg(filename)
  File.expand_path("../pkg/#{filename}", __FILE__)
end

def project_root
  File.dirname(__FILE__)
end

def resource(name)
  File.expand_path("../dist/resources/#{name}", __FILE__)
end

def s3_connect
  return if @s3_connected

  require "aws/s3"

  unless ENV["DAVID_RELEASE_ACCESS"] && ENV["DAVID_RELEASE_SECRET"]
    puts "please set DAVID_RELEASE_ACCESS and DAVID_RELEASE_SECRET in your environment"
    exit 1
  end

  AWS::S3::Base.establish_connection!(
    :access_key_id => ENV["DAVID_RELEASE_ACCESS"],
    :secret_access_key => ENV["DAVID_RELEASE_SECRET"]
  )

  @s3_connected = true
end

def store(package_file, filename, bucket="assets.foreman.io")
  s3_connect
  puts "storing: #{filename}"
  AWS::S3::S3Object.store(filename, File.open(package_file), bucket, :access => :public_read)
end

def tempdir
  Dir.mktmpdir do |dir|
    Dir.chdir(dir) do
      yield(dir)
    end
  end
end

def version
  "0.1.0"
end

Dir[File.expand_path("../dist/**/*.rake", __FILE__)].each do |rake|
  import rake
end
