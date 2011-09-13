file EXECUTABLE => source_files do |t|
  sh %{ msbuild Foreman.sln /p:Configuration=Release }
end

file pkg("foreman-#{version}.exe") => EXECUTABLE do |t|
  tempdir do |dir|
    mkchdir("foreman") do
      cp EXECUTABLE, "Foreman.exe"
    end

    File.open("foreman.iss", "w") do |iss|
      iss.write(ERB.new(File.read(resource("exe/foreman.iss"))).result(binding))
    end

    inno_dir = ENV["INNO_DIR"] || 'C:\\Program Files (x86)\\Inno Setup 5\\'

    system "\"#{inno_dir}\\Compil32.exe\" /cc \"foreman.iss\""
  end
end

task "exe:build" => pkg("foreman-#{version}.exe")

task "exe:clean" do
  sh %{ msbuild Foreman.sln /t:Clean }
  clean pkg("foreman-#{version}.exe")
end

task "exe:release" => "exe:build" do |t|
  store pkg("foreman-#{version}.exe"), "foreman/foreman-setup-#{version}.exe"
  store pkg("foreman-#{version}.exe"), "foreman/foreman-setup.exe"
end
